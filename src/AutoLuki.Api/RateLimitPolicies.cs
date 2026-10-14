using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace AutoLuki.Api;

public class RateLimitOptions
{
    public const string SectionName = "RateLimits";

    public int LoginPermitLimit { get; init; } = 10;
    public int LoginWindowMinutes { get; init; } = 5;
    public int InquiryPermitLimit { get; init; } = 5;
    public int InquiryWindowMinutes { get; init; } = 10;
}

public static class RateLimitPolicies
{
    public const string Login = "login";
    public const string Inquiry = "inquiry";

    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var limits = configuration.GetSection(RateLimitOptions.SectionName).Get<RateLimitOptions>() ?? new RateLimitOptions();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(Login, context => RateLimitPartition.GetFixedWindowLimiter(
                ClientIp(context),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = limits.LoginPermitLimit,
                    Window = TimeSpan.FromMinutes(limits.LoginWindowMinutes),
                }));

            options.AddPolicy(Inquiry, context => RateLimitPartition.GetFixedWindowLimiter(
                ClientIp(context),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = limits.InquiryPermitLimit,
                    Window = TimeSpan.FromMinutes(limits.InquiryWindowMinutes),
                }));
        });

        return services;
    }

    private static string ClientIp(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
