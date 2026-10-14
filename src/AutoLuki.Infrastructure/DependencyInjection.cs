using System.Text;
using AutoLuki.Application.Auth;
using AutoLuki.Application.Common;
using AutoLuki.Infrastructure.Captcha;
using AutoLuki.Infrastructure.Email;
using AutoLuki.Infrastructure.Identity;
using AutoLuki.Infrastructure.Persistence;
using AutoLuki.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AutoLuki.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddPersistence(configuration);
        services.AddIdentityAndJwt(configuration);
        services.AddStorage(configuration);
        services.AddCaptchaAndEmail(configuration);

        return services;
    }

    private static void AddCaptchaAndEmail(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<TurnstileOptions>()
            .Bind(configuration.GetSection(TurnstileOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.SecretKey), "Turnstile:SecretKey is required.")
            .ValidateOnStart();
        services.AddHttpClient<ICaptchaVerifier, TurnstileVerifier>(client => client.Timeout = TimeSpan.FromSeconds(10));

        var emailSection = configuration.GetSection(EmailOptions.SectionName);
        services.Configure<EmailOptions>(emailSection);
        if (emailSection.Get<EmailOptions>()?.IsConfigured == true)
        {
            services.AddTransient<IEmailSender, SmtpEmailSender>();
        }
        else
        {
            services.AddTransient<IEmailSender, LoggingEmailSender>();
        }

        services.AddSingleton<InquiryNotificationQueue>();
        services.AddSingleton<IInquiryNotificationQueue>(sp => sp.GetRequiredService<InquiryNotificationQueue>());
        services.AddHostedService<InquiryNotificationWorker>();
    }

    private static void AddStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(StorageOptions.SectionName);
        services.Configure<StorageOptions>(section);

        services.AddSingleton<IImageProcessor, SkiaImageProcessor>();

        var provider = section.GetValue<string>(nameof(StorageOptions.Provider)) ?? "Local";
        switch (provider)
        {
            case "Local":
                services.AddSingleton<LocalFileStorage>();
                services.AddSingleton<IFileStorage>(sp => sp.GetRequiredService<LocalFileStorage>());
                break;
            case "Azure":
                services.AddSingleton<IFileStorage, AzureBlobFileStorage>();
                break;
            default:
                throw new InvalidOperationException($"Unknown Storage:Provider '{provider}'. Use 'Local' or 'Azure'.");
        }
    }

    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Default' is not configured.");
        }

        services.AddScoped<TimestampInterceptor>();
        services.AddDbContext<AppDbContext>((sp, options) => options
            .UseNpgsql(connectionString)
            .AddInterceptors(sp.GetRequiredService<TimestampInterceptor>()));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
    }

    private static void AddIdentityAndJwt(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityCore<AdminUser>(options =>
            {
                options.Password.RequiredLength = 10;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwt) =>
            {
                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Value.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Value.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Value.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                };
            });

        services.AddScoped<IAuthService, AuthService>();
    }
}
