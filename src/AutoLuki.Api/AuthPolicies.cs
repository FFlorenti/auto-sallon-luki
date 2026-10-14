using AutoLuki.Application.Common;

namespace AutoLuki.Api;

public static class AuthPolicies
{
    public const string StaffOrAdmin = "StaffOrAdmin";

    public const string AdminOnly = "AdminOnly";

    public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(StaffOrAdmin, policy => policy.RequireRole(Roles.Admin, Roles.Staff))
            .AddPolicy(AdminOnly, policy => policy.RequireRole(Roles.Admin));

        return services;
    }
}
