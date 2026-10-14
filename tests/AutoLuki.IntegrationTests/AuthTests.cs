using System.Net;
using System.Net.Http.Json;
using AutoLuki.Application.Auth;
using AutoLuki.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace AutoLuki.IntegrationTests;

public class AuthTests(PostgresFixture postgres) : IntegrationTestBase(postgres)
{
    [Fact]
    public async Task Health_check_reports_healthy_database()
    {
        var response = await Anonymous.GetStringAsync("/health");

        Assert.Equal("Healthy", response);
    }

    [Fact]
    public async Task Login_with_seeded_admin_returns_token_with_admin_role()
    {
        var response = await Anonymous.PostAsJsonAsync("/api/auth/login", new LoginRequest(ApiFactory.AdminEmail, ApiFactory.AdminPassword));

        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>(ApiFactory.Json);
        Assert.False(string.IsNullOrEmpty(login!.AccessToken));
        Assert.Contains("Admin", login.Roles);
    }

    [Fact]
    public async Task Wrong_password_and_unknown_email_get_the_same_401()
    {
        var wrongPassword = await Anonymous.PostAsJsonAsync("/api/auth/login", new LoginRequest(ApiFactory.AdminEmail, "wrong-password"));
        var unknownEmail = await Anonymous.PostAsJsonAsync("/api/auth/login", new LoginRequest("nobody@test.local", "whatever-123"));

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownEmail.StatusCode);
        var a = await wrongPassword.Content.ReadFromJsonAsync<ProblemDetails>();
        var b = await unknownEmail.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(a!.Title, b!.Title);
    }

    [Fact]
    public async Task Invalid_login_input_returns_400_with_field_errors()
    {
        var response = await Anonymous.PostAsJsonAsync("/api/auth/login", new LoginRequest("not-an-email", ""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"Email\"", body);
        Assert.Contains("\"Password\"", body);
    }

    [Theory]
    [InlineData("/api/admin/dashboard")]
    [InlineData("/api/admin/cars")]
    [InlineData("/api/admin/inquiries")]
    public async Task Admin_endpoints_require_a_token(string url)
    {
        var response = await Anonymous.GetAsync(url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Forged_token_is_rejected()
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer",
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ4Iiwicm9sZSI6IkFkbWluIn0.c2lnbmF0dXJl");

        var response = await client.GetAsync("/api/admin/dashboard");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Staff_can_manage_cars_but_cannot_delete_them()
    {
        var staff = await Factory.CreateStaffClientAsync();
        var modelId = await CreateUniqueModelAsync();

        var create = await staff.PostAsJsonAsync("/api/admin/cars", NewCar(modelId), ApiFactory.Json);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var id = (await create.Content.ReadFromJsonAsync<IdResponse>(ApiFactory.Json))!.Id;

        var delete = await staff.DeleteAsync($"/api/admin/cars/{id}");
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);

        var adminDelete = await Admin.DeleteAsync($"/api/admin/cars/{id}");
        Assert.Equal(HttpStatusCode.NoContent, adminDelete.StatusCode);
    }
}
