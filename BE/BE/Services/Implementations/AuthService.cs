using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BE.Data;
using BE.DTOs;
using BE.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BE.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest req);
    Task<AuthResponse> LoginAsync(LoginRequest req);
}

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _cfg;

    private readonly PasswordHasher<Users> _hasher = new();

    public AuthService(ApplicationDbContext db, IConfiguration cfg)
    {
        _db = db;
        _cfg = cfg;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req)
    {
        var email = (req.Email ?? "").Trim().ToLower();
        if (string.IsNullOrWhiteSpace(email)) throw new Exception("Email không được để trống.");
        if (string.IsNullOrWhiteSpace(req.Password)) throw new Exception("Password không được để trống.");

        var exists = await _db.Users.AnyAsync(u => u.Email.ToLower() == email);
        if (exists) throw new Exception("Email đã tồn tại.");

        var roleName = (req.Role ?? "customer").Trim().ToLower();
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName!.ToLower() == roleName);
        if (role == null) throw new Exception($"Role '{roleName}' không tồn tại trong bảng roles.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        try
        {
            var user = new Users
            {
                Email = email,
                FullName = req.FullName,
                Phone = req.Phone,
                Status = "ACTIVE",
            };
            user.PasswordHash = _hasher.HashPassword(user, req.Password);

            // Add user + role (insert users + user_roles)
            user.Role.Add(role);
            _db.Users.Add(user);
            await _db.SaveChangesAsync(); // ✅ user.Id có ở đây

            long? providerId = null;

            // ✅ Nếu role = provider thì tạo providers row
            if (roleName == "provider")
            {
                // tránh tạo trùng
                providerId = await _db.Providers
                    .Where(p => p.UserId == user.Id)
                    .Select(p => (long?)p.Id)
                    .FirstOrDefaultAsync();

                if (!providerId.HasValue)
                {
                    var provider = new Providers
                    {
                        UserId = user.Id,
                        BrandName = string.IsNullOrWhiteSpace(user.FullName) ? "Provider" : user.FullName,
                        ProviderType = "OUTFIT",
                        Status = "ACTIVE",
                        Verified = true // hoặc false tuỳ bạn
                    };

                    _db.Providers.Add(provider);
                    await _db.SaveChangesAsync();

                    providerId = provider.Id;
                }
            }

            await tx.CommitAsync();

            // role cuối cùng (nếu bạn muốn ưu tiên admin/provider/customer)
            var finalRole = roleName;

            var token = CreateJwt(user, finalRole, providerId);

            return new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                Role = finalRole,
                ProviderId = providerId,
                FullName = user.FullName,
                Token = token,
                RedirectUrl = null
            };
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            throw new Exception(ex.InnerException?.Message ?? ex.Message);
        }
    }




    public async Task<AuthResponse> LoginAsync(LoginRequest req)
    {
        var email = (req.Email ?? "").Trim().ToLower();
        if (string.IsNullOrWhiteSpace(email)) throw new Exception("Email không được để trống.");
        if (string.IsNullOrWhiteSpace(req.Password)) throw new Exception("Password không được để trống.");

        // Include Role để lấy role ngay
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

        if (user == null) throw new Exception("Sai email hoặc mật khẩu.");

        if (!string.IsNullOrEmpty(user.Status) &&
            !user.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("Tài khoản đang bị khóa hoặc không hoạt động.");
        }

        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
        if (verify == PasswordVerificationResult.Failed) throw new Exception("Sai email hoặc mật khẩu.");

        // lấy role tốt nhất
        var finalRole = PickBestRole(user.Role.Select(r => r.RoleName));
        if (string.IsNullOrWhiteSpace(finalRole))
            finalRole = await GetBestRoleAsync(user.Id);

        // ✅ nếu là provider -> lấy providerId
        long? providerId = null;
        if (finalRole.Equals("provider", StringComparison.OrdinalIgnoreCase))
        {
            providerId = await GetProviderIdAsync(user.Id);
        }

        // ✅ token có claim providerId (nếu có)
        var token = CreateJwt(user, finalRole, providerId);

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Role = finalRole,
            ProviderId = providerId,
            FullName = user.FullName,
            Token = token,
            RedirectUrl = MapRedirect(finalRole)
        };
    }

    // ====== ROLE PICKER ======
    private static string PickBestRole(IEnumerable<string?> roleNames)
    {
        var list = roleNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .ToList();

        bool has(string r) => list.Any(x => x.Equals(r, StringComparison.OrdinalIgnoreCase));

        if (has("admin")) return "admin";
        if (has("provider")) return "provider";
        if (has("customer")) return "customer";
        return "customer";
    }

    private async Task<string> GetBestRoleAsync(long userId)
    {
        var roleNames = await _db.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Role.Select(r => r.RoleName))
            .ToListAsync();

        return PickBestRole(roleNames);
    }

    // ====== ✅ PROVIDER ID ======
    private async Task<long?> GetProviderIdAsync(long userId)
    {
        // Providers.UserId là FK -> users.id (đúng theo DbContext bạn gửi)
        return await _db.Providers
            .Where(p => p.UserId == userId)
            .Select(p => (long?)p.Id)
            .FirstOrDefaultAsync();
    }

    // ====== ✅ JWT (có providerId claim) ======
    private string CreateJwt(Users user, string role, long? providerId)
    {
        var key = _cfg["Jwt:Key"];
        var issuer = _cfg["Jwt:Issuer"];
        var audience = _cfg["Jwt:Audience"];
        var expMinStr = _cfg["Jwt:ExpireMinutes"];

        if (string.IsNullOrWhiteSpace(key)) throw new Exception("Thiếu Jwt:Key trong appsettings.json");
        if (key.Length < 32) throw new Exception("Jwt:Key phải >= 32 ký tự");
        if (string.IsNullOrWhiteSpace(issuer)) throw new Exception("Thiếu Jwt:Issuer trong appsettings.json");
        if (string.IsNullOrWhiteSpace(audience)) throw new Exception("Thiếu Jwt:Audience trong appsettings.json");
        if (!int.TryParse(expMinStr, out var expMin)) expMin = 120;

        var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // ✅ chuẩn
    new Claim(ClaimTypes.Email, user.Email),                  // ✅ chuẩn
    new Claim(ClaimTypes.Role, role),                         // ✅ chuẩn
};

        if (providerId.HasValue)
        {
            claims.Add(new Claim("providerId", providerId.Value.ToString()));
        }


        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expMin),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string MapRedirect(string role)
    {
        role = (role ?? "customer").Trim().ToLower();

        return role switch
        {
            "admin" => "../Admin/admin-dashboard/index.html",
            "provider" => "../Manager/index.html",
            "customer" => "../index.html",
            _ => "../index.html"
        };
    }

}
