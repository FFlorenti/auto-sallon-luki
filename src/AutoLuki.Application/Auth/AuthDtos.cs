namespace AutoLuki.Application.Auth;

public record LoginRequest(string Email, string Password);

public record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt, string Email, IReadOnlyList<string> Roles);
