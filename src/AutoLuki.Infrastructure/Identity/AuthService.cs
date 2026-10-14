using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoLuki.Application.Auth;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AutoLuki.Infrastructure.Identity;

public class AuthService(
    UserManager<AdminUser> userManager,
    SignInManager<AdminUser> signInManager,
    IValidator<LoginRequest> validator,
    IOptions<JwtOptions> jwtOptions,
    TimeProvider timeProvider,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            logger.LogWarning("Login failed: unknown email");
            return null;
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            logger.LogWarning("Login failed for user {UserId}. Locked out: {IsLockedOut}", user.Id, result.IsLockedOut);
            return null;
        }

        var roles = (await userManager.GetRolesAsync(user)).ToList();
        var (token, expiresAt) = CreateToken(user, roles);

        logger.LogInformation("User {UserId} logged in", user.Id);
        return new LoginResponse(token, expiresAt, user.Email!, roles);
    }

    private (string Token, DateTimeOffset ExpiresAt) CreateToken(AdminUser user, IEnumerable<string> roles)
    {
        var options = jwtOptions.Value;
        var now = timeProvider.GetUtcNow();
        var expiresAt = now.AddMinutes(options.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey));
        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
