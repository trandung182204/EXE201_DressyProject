namespace BE.DTOs;

public class RegisterRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? FullName { get; set; }
    public string? Phone { get; set; }

    // nếu bạn không truyền => mặc định customer
    public string? Role { get; set; } // "customer" | "admin" | "provider"
}

public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class AuthResponse
{
    public long UserId { get; set; }
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string RedirectUrl { get; set; } = null!;
    public long? ProviderId { get; set; }
}

/// <summary>
/// Response DTO for GET /api/auth/me endpoint
/// </summary>
public class UserProfileResponse
{
    public long UserId { get; set; }
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string? FullName { get; set; }
    public long? ProviderId { get; set; }
}

