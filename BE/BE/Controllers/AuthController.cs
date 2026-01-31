using BE.DTOs;
using BE.Services;
using BE.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BE.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ApplicationDbContext _db;

    public AuthController(IAuthService auth, ApplicationDbContext db)
    {
        _auth = auth;
        _db = db;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req)
    {
        try
        {
            return Ok(await _auth.RegisterAsync(req));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
    {
        try
        {
            return Ok(await _auth.LoginAsync(req));
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get current user profile from JWT token
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserProfileResponse>> GetProfile()
    {
        try
        {
            // Extract claims from JWT
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                              ?? User.FindFirst(ClaimTypes.NameIdentifier);
            var emailClaim = User.FindFirst(JwtRegisteredClaimNames.Email)
                             ?? User.FindFirst(ClaimTypes.Email);
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            var providerIdClaim = User.FindFirst("providerId");

            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new { message = "Invalid token: missing user ID" });
            }

            // Query DB for fullName
            var user = await _db.Users
    .AsNoTracking()
    .Where(u => u.Id == userId)
    .Select(u => new { u.FullName, u.Phone })
    .FirstOrDefaultAsync();


            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var response = new UserProfileResponse
            {
                UserId = userId,
                Email = emailClaim?.Value ?? "",
                Role = roleClaim?.Value ?? "customer",
                FullName = user.FullName,
                ProviderId = long.TryParse(providerIdClaim?.Value, out var pid) ? pid : null,
                Phone = user.Phone
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}

