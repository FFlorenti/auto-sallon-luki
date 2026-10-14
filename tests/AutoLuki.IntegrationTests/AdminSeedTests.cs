using System.Net;
using System.Net.Http.Json;
using AutoLuki.Application.Auth;
using AutoLuki.IntegrationTests.Infrastructure;

namespace AutoLuki.IntegrationTests;

[Collection(IntegrationCollection.Name)]
public class AdminSeedTests(PostgresFixture postgres)
{
    private static async Task<HttpStatusCode> LoginAsync(ApiFactory api, string email, string password) =>
        (await api.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password))).StatusCode;

    private ApiFactory Start(string database, string? email = null, string? password = null) =>
        new(postgres, database, new Dictionary<string, string?>
        {
            ["AdminSeed:Email"] = email ?? ApiFactory.AdminEmail,
            ["AdminSeed:Password"] = password ?? ApiFactory.AdminPassword,
        });

    [Fact]
    public async Task Changing_the_configured_password_changes_the_login_after_restart()
    {
        const string db = "autoluki_seed_password";
        await using (var first = Start(db))
        {
            Assert.Equal(HttpStatusCode.OK, await LoginAsync(first, ApiFactory.AdminEmail, ApiFactory.AdminPassword));
        }

        await using var restarted = Start(db, password: "Brand-New-Password-456");

        Assert.Equal(HttpStatusCode.OK, await LoginAsync(restarted, ApiFactory.AdminEmail, "Brand-New-Password-456"));
        Assert.Equal(HttpStatusCode.Unauthorized, await LoginAsync(restarted, ApiFactory.AdminEmail, ApiFactory.AdminPassword));
    }

    [Fact]
    public async Task Changing_the_configured_email_replaces_the_admin_after_restart()
    {
        const string db = "autoluki_seed_email";
        await using (var first = Start(db))
        {
            Assert.Equal(HttpStatusCode.OK, await LoginAsync(first, ApiFactory.AdminEmail, ApiFactory.AdminPassword));
        }

        await using var restarted = Start(db, email: "owner@test.local");

        Assert.Equal(HttpStatusCode.OK, await LoginAsync(restarted, "owner@test.local", ApiFactory.AdminPassword));
        Assert.Equal(HttpStatusCode.Unauthorized, await LoginAsync(restarted, ApiFactory.AdminEmail, ApiFactory.AdminPassword));
    }

    [Fact]
    public async Task New_email_with_a_weak_password_keeps_the_old_admin()
    {
        const string db = "autoluki_seed_lockout";
        await using (var first = Start(db))
        {
            Assert.Equal(HttpStatusCode.OK, await LoginAsync(first, ApiFactory.AdminEmail, ApiFactory.AdminPassword));
        }

        await using var restarted = Start(db, email: "owner@test.local", password: "weakpass");

        Assert.Equal(HttpStatusCode.Unauthorized, await LoginAsync(restarted, "owner@test.local", "weakpass"));
        Assert.Equal(HttpStatusCode.OK, await LoginAsync(restarted, ApiFactory.AdminEmail, ApiFactory.AdminPassword));
    }

    [Fact]
    public async Task Weak_configured_password_is_refused_but_the_site_still_starts()
    {
        const string db = "autoluki_seed_weak";
        await using (var first = Start(db))
        {
            Assert.Equal(HttpStatusCode.OK, await LoginAsync(first, ApiFactory.AdminEmail, ApiFactory.AdminPassword));
        }

        await using var restarted = Start(db, password: "weakpass");

        Assert.Equal("Healthy", await restarted.CreateClient().GetStringAsync("/health"));
        Assert.Equal(HttpStatusCode.Unauthorized, await LoginAsync(restarted, ApiFactory.AdminEmail, "weakpass"));
        Assert.Equal(HttpStatusCode.OK, await LoginAsync(restarted, ApiFactory.AdminEmail, ApiFactory.AdminPassword));
    }
}
