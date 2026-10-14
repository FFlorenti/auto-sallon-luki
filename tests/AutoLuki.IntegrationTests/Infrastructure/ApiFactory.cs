using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoLuki.Application.Auth;
using AutoLuki.Application.Common;
using AutoLuki.Infrastructure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AutoLuki.IntegrationTests.Infrastructure;

public class ApiFactory(PostgresFixture postgres, string database, IDictionary<string, string?>? extraSettings = null)
    : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@test.local";
    public const string AdminPassword = "Admin-Password-123";
    public const string StaffEmail = "staff@test.local";
    public const string StaffPassword = "Staff-Password-123";

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private readonly string _uploads = Path.Combine(Path.GetTempPath(), "autoluki-tests", Guid.NewGuid().ToString("N"));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", postgres.ConnectionString(database));
        builder.UseSetting("Database:MigrateOnStartup", "true");
        builder.UseSetting("Jwt:SigningKey", "integration-tests-signing-key-0123456789abcdef");
        builder.UseSetting("AdminSeed:Email", AdminEmail);
        builder.UseSetting("AdminSeed:Password", AdminPassword);
        builder.UseSetting("Turnstile:SecretKey", "unused-fake-is-registered");
        builder.UseSetting("Storage:LocalPath", _uploads);
        builder.UseSetting("RateLimits:InquiryPermitLimit", "1000");
        builder.UseSetting("RateLimits:LoginPermitLimit", "1000");
        builder.UseSetting("Serilog:MinimumLevel:Default", "Warning");
        foreach (var (key, value) in extraSettings ?? new Dictionary<string, string?>())
        {
            builder.UseSetting(key, value);
        }

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ICaptchaVerifier>();
            services.AddSingleton<ICaptchaVerifier, FakeCaptchaVerifier>();
        });
    }

    public async Task<HttpClient> CreateAdminClientAsync() => await CreateClientAsync(AdminEmail, AdminPassword);

    public async Task<HttpClient> CreateStaffClientAsync()
    {
        using (var scope = Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<AdminUser>>();
            if (await users.FindByEmailAsync(StaffEmail) is null)
            {
                var staff = new AdminUser { UserName = StaffEmail, Email = StaffEmail, EmailConfirmed = true };
                await users.CreateAsync(staff, StaffPassword);
                await users.AddToRoleAsync(staff, Roles.Staff);
            }
        }

        return await CreateClientAsync(StaffEmail, StaffPassword);
    }

    private async Task<HttpClient> CreateClientAsync(string email, string password)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>(Json);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && Directory.Exists(_uploads))
        {
            Directory.Delete(_uploads, recursive: true);
        }
    }
}

public class FakeCaptchaVerifier : ICaptchaVerifier
{
    public const string ValidToken = "valid";

    public Task<bool> VerifyAsync(string token, string? remoteIp, CancellationToken cancellationToken = default) =>
        Task.FromResult(token == ValidToken);
}
