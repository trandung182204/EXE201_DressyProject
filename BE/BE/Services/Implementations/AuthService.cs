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

    // Dùng PasswordHasher để hash/verify password (không cần Identity full)
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

        var user = new Users
        {
            Email = email,
            FullName = req.FullName,
            Phone = req.Phone,
            Status = "active",
        };

        user.PasswordHash = _hasher.HashPassword(user, req.Password);

        // role mặc định
        var roleName = (req.Role ?? "customer").Trim().ToLower();
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName!.ToLower() == roleName);
        if (role == null) throw new Exception($"Role '{roleName}' không tồn tại trong bảng roles.");

        // Many-to-many: Users.Role <-> Roles.User
        user.Role.Add(role);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Sau SaveChanges, user đã có Id
        // Lấy role tốt nhất (admin > provider > customer)
        var finalRole = await GetBestRoleAsync(user.Id);

        var token = CreateJwt(user, finalRole);

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Role = finalRole,
            Token = token,
            RedirectUrl = MapRedirect(finalRole)
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req)
    {
        var email = (req.Email ?? "").Trim().ToLower();
        if (string.IsNullOrWhiteSpace(email)) throw new Exception("Email không được để trống.");
        if (string.IsNullOrWhiteSpace(req.Password)) throw new Exception("Password không được để trống.");

        // Include Role để khỏi query lại nhiều lần
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

        if (user == null) throw new Exception("Sai email hoặc mật khẩu.");

        // (Tuỳ bạn) chặn account bị khóa
        if (!string.IsNullOrEmpty(user.Status) &&
            !user.Status.Equals("active", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("Tài khoản đang bị khóa hoặc không hoạt động.");
        }

        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
        if (verify == PasswordVerificationResult.Failed) throw new Exception("Sai email hoặc mật khẩu.");

        // Vì đã Include Role nên lấy luôn role từ navigation (không cần _db.UserRoles)
        var finalRole = PickBestRole(user.Role.Select(r => r.RoleName));

        // fallback nếu vì lý do nào đó chưa load roleNames
        if (string.IsNullOrWhiteSpace(finalRole))
            finalRole = await GetBestRoleAsync(user.Id);

        var token = CreateJwt(user, finalRole);

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Role = finalRole,
            Token = token,
            RedirectUrl = MapRedirect(finalRole)
        };
    }

    /// <summary>
    /// Lấy role từ navigation Users.Role (many-to-many) - ưu tiên admin > provider > customer
    /// </summary>
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

    /// <summary>
    /// Query role từ DB (trường hợp không Include Role)
    /// </summary>
    private async Task<string> GetBestRoleAsync(long userId)
    {
        var roleNames = await _db.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Role.Select(r => r.RoleName))
            .ToListAsync();

        return PickBestRole(roleNames);
    }

    private string CreateJwt(Users user, string role)
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
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, role),
        };

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
            "customer" => "FE/bean-style.mysapo.net/index.html",
            "admin" => "FE/Admin/admin-dashboard/index.html",
            "provider" => "FE/Manager/ExeManager/nta0309-ecommerce-admin-dashboard.netlify.app/index.html",
            _ => "FE/bean-style.mysapo.net/index.html"
        };
    }
}
