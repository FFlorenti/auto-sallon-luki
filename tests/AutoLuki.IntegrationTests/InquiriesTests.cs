using System.Net;
using System.Net.Http.Json;
using AutoLuki.Application.Common;
using AutoLuki.Application.Dashboard;
using AutoLuki.Application.Inquiries;
using AutoLuki.Domain;
using AutoLuki.IntegrationTests.Infrastructure;

namespace AutoLuki.IntegrationTests;

public class InquiriesTests(PostgresFixture postgres) : IntegrationTestBase(postgres)
{
    private static CreateInquiryRequest Inquiry(int? carId, string token = FakeCaptchaVerifier.ValidToken, InquiryType type = InquiryType.General) =>
        new(carId, type, "Arben Krasniqi", "+383 44 123 456", null, "A është ende në shitje?", null, token);

    [Fact]
    public async Task Valid_inquiry_is_accepted_and_shows_up_for_the_admin()
    {
        var carId = await CreateCarAsync(NewCar(await CreateUniqueModelAsync()));

        var response = await Anonymous.PostAsJsonAsync("/api/inquiries", Inquiry(carId), ApiFactory.Json);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var inbox = await Admin.GetFromJsonAsync<PagedResult<InquiryDto>>("/api/admin/inquiries?status=New&pageSize=100", ApiFactory.Json);
        var inquiry = Assert.Single(inbox!.Items, i => i.Car?.Id == carId);
        Assert.Equal("A është ende në shitje?", inquiry.Message);
    }

    [Fact]
    public async Task Failed_spam_check_is_rejected_and_nothing_is_saved()
    {
        var carId = await CreateCarAsync(NewCar(await CreateUniqueModelAsync()));

        var response = await Anonymous.PostAsJsonAsync("/api/inquiries", Inquiry(carId, token: "bot"), ApiFactory.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var inbox = await Admin.GetFromJsonAsync<PagedResult<InquiryDto>>("/api/admin/inquiries?pageSize=100", ApiFactory.Json);
        Assert.DoesNotContain(inbox!.Items, i => i.Car?.Id == carId);
    }

    [Fact]
    public async Task Inquiry_for_unknown_car_is_rejected()
    {
        var response = await Anonymous.PostAsJsonAsync("/api/inquiries", Inquiry(carId: 999_999), ApiFactory.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Test_drive_without_car_is_rejected()
    {
        var response = await Anonymous.PostAsJsonAsync("/api/inquiries", Inquiry(null, type: InquiryType.TestDrive), ApiFactory.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Admin_can_change_status_and_dashboard_counts_follow()
    {
        await Anonymous.PostAsJsonAsync("/api/inquiries", Inquiry(null), ApiFactory.Json);
        var before = await Admin.GetFromJsonAsync<DashboardDto>("/api/admin/dashboard", ApiFactory.Json);
        var inquiry = (await Admin.GetFromJsonAsync<PagedResult<InquiryDto>>("/api/admin/inquiries?status=New", ApiFactory.Json))!.Items[0];

        var response = await Admin.PatchAsJsonAsync($"/api/admin/inquiries/{inquiry.Id}/status",
            new ChangeInquiryStatusRequest(InquiryStatus.Contacted), ApiFactory.Json);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var after = await Admin.GetFromJsonAsync<DashboardDto>("/api/admin/dashboard", ApiFactory.Json);
        Assert.Equal(before!.InquiriesNew - 1, after!.InquiriesNew);
        Assert.Equal(before.InquiriesTotal, after.InquiriesTotal);
    }
}

[Collection(IntegrationCollection.Name)]
public class RateLimitTests(PostgresFixture postgres)
{
    private readonly ApiFactory _factory = postgres.GetFactory("autoluki_ratelimit",
        new Dictionary<string, string?> { ["RateLimits:InquiryPermitLimit"] = "2" });

    [Fact]
    public async Task Too_many_inquiries_from_one_client_get_429()
    {
        var client = _factory.CreateClient();
        var request = new CreateInquiryRequest(null, InquiryType.General, "Bot", "044123456", null, "spam", null, FakeCaptchaVerifier.ValidToken);

        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 3; i++)
        {
            statuses.Add((await client.PostAsJsonAsync("/api/inquiries", request, ApiFactory.Json)).StatusCode);
        }

        Assert.Equal([HttpStatusCode.Accepted, HttpStatusCode.Accepted, HttpStatusCode.TooManyRequests], statuses);
    }
}
