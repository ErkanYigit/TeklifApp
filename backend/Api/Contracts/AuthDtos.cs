public record RegisterRequest(string Email, string UserCode, string Password);
public record LoginRequest(string UserCode, string Password);
public record TokenResponse(string Token, string RefreshToken, DateTime ExpiresAt, string Role);