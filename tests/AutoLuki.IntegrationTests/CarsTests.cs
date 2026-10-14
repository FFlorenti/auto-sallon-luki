using System.Net;
using System.Net.Http.Json;
using AutoLuki.Application.Cars;
using AutoLuki.Application.Common;
using AutoLuki.Domain;
using AutoLuki.IntegrationTests.Infrastructure;

namespace AutoLuki.IntegrationTests;

public class CarsTests(PostgresFixture postgres) : IntegrationTestBase(postgres)
{
    private Task<PagedResult<CarListItemDto>?> SearchAsync(string query) =>
        Anonymous.GetFromJsonAsync<PagedResult<CarListItemDto>>($"/api/cars?{query}", ApiFactory.Json);

    [Fact]
    public async Task Created_car_is_visible_on_the_public_site()
    {
        var modelId = await CreateUniqueModelAsync();
        var id = await CreateCarAsync(NewCar(modelId, price: 12_345m));

        var car = await Anonymous.GetFromJsonAsync<CarDetailDto>($"/api/cars/{id}", ApiFactory.Json);

        Assert.Equal(12_345m, car!.Price);
        Assert.Equal(CarStatus.Available, car.Status);
        Assert.Equal("Përshkrim", car.DescriptionSq);
    }

    [Fact]
    public async Task Public_api_never_exposes_the_vin()
    {
        var id = await CreateCarAsync(NewCar(await CreateUniqueModelAsync()));

        var json = await Anonymous.GetStringAsync($"/api/cars/{id}");
        var adminJson = await Admin.GetStringAsync($"/api/admin/cars/{id}");

        Assert.DoesNotContain("vin", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WVWZZZ3CZJE123456", adminJson);
    }

    [Fact]
    public async Task Filters_sorting_and_paging_work_together()
    {
        var modelId = await CreateUniqueModelAsync();
        foreach (var price in new[] { 9_000m, 11_000m, 13_000m, 15_000m, 30_000m })
        {
            await CreateCarAsync(NewCar(modelId, price));
        }

        var page1 = await SearchAsync($"modelId={modelId}&priceFrom=10000&priceTo=20000&sort=PriceAsc&pageSize=2&page=1");
        var page2 = await SearchAsync($"modelId={modelId}&priceFrom=10000&priceTo=20000&sort=PriceAsc&pageSize=2&page=2");

        Assert.Equal(3, page1!.TotalCount);
        Assert.Equal(2, page1.TotalPages);
        Assert.Equal([11_000m, 13_000m], page1.Items.Select(c => c.Price));
        Assert.Equal([15_000m], page2!.Items.Select(c => c.Price));
    }

    [Fact]
    public async Task Text_search_matches_brand_and_model_case_insensitively()
    {
        var modelId = await CreateUniqueModelAsync();
        await CreateCarAsync(NewCar(modelId));
        var car = (await SearchAsync($"modelId={modelId}"))!.Items.Single();

        var result = await SearchAsync($"search={Uri.EscapeDataString(car.Model.ToUpperInvariant())}");

        Assert.Contains(result!.Items, c => c.Model == car.Model);
    }

    [Fact]
    public async Task Sold_cars_disappear_from_the_public_list_but_their_page_still_works()
    {
        var modelId = await CreateUniqueModelAsync();
        var id = await CreateCarAsync(NewCar(modelId));

        var status = await Admin.PatchAsJsonAsync($"/api/admin/cars/{id}/status", new ChangeCarStatusRequest(CarStatus.Sold), ApiFactory.Json);
        Assert.Equal(HttpStatusCode.NoContent, status.StatusCode);

        Assert.Equal(0, (await SearchAsync($"modelId={modelId}"))!.TotalCount);
        var detail = await Anonymous.GetFromJsonAsync<CarDetailDto>($"/api/cars/{id}", ApiFactory.Json);
        Assert.Equal(CarStatus.Sold, detail!.Status);

        var admin = await Admin.GetFromJsonAsync<AdminCarDetailDto>($"/api/admin/cars/{id}", ApiFactory.Json);
        Assert.NotNull(admin!.SoldAt);
    }

    [Fact]
    public async Task Deleted_car_returns_404_everywhere()
    {
        var id = await CreateCarAsync(NewCar(await CreateUniqueModelAsync()));

        await Admin.DeleteAsync($"/api/admin/cars/{id}");

        Assert.Equal(HttpStatusCode.NotFound, (await Anonymous.GetAsync($"/api/cars/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Admin.GetAsync($"/api/admin/cars/{id}")).StatusCode);
    }

    [Fact]
    public async Task Invalid_car_returns_400_with_errors_per_field()
    {
        var request = NewCar(await CreateUniqueModelAsync()) with { Year = 1900, Price = -1 };

        var response = await Admin.PostAsJsonAsync("/api/admin/cars", request, ApiFactory.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"Year\"", body);
        Assert.Contains("\"Price\"", body);
    }

    [Fact]
    public async Task Car_with_unknown_model_is_rejected()
    {
        var response = await Admin.PostAsJsonAsync("/api/admin/cars", NewCar(modelId: 999_999), ApiFactory.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Page_size_is_capped()
    {
        var response = await Anonymous.GetAsync("/api/cars?pageSize=1000");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Timestamps_are_set_automatically()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-5);
        var id = await CreateCarAsync(NewCar(await CreateUniqueModelAsync()));

        var car = await Admin.GetFromJsonAsync<AdminCarDetailDto>($"/api/admin/cars/{id}", ApiFactory.Json);

        Assert.True(car!.CreatedAt > before);
        Assert.Equal(car.CreatedAt, car.UpdatedAt);
    }
}
