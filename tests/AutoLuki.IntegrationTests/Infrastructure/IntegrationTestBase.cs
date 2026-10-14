using System.Net.Http.Json;
using AutoLuki.Application.Brands;
using AutoLuki.Application.Cars;
using AutoLuki.Domain;

namespace AutoLuki.IntegrationTests.Infrastructure;

[Collection(IntegrationCollection.Name)]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected IntegrationTestBase(PostgresFixture postgres, IDictionary<string, string?>? settings = null)
    {
        Postgres = postgres;
        Factory = postgres.GetFactory("autoluki_" + GetType().Name.ToLowerInvariant(), settings);
    }

    protected PostgresFixture Postgres { get; }

    protected ApiFactory Factory { get; }
    protected HttpClient Anonymous { get; private set; } = null!;
    protected HttpClient Admin { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Anonymous = Factory.CreateClient();
        Admin = await Factory.CreateAdminClientAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task<int> CreateUniqueModelAsync()
    {
        var brands = await Anonymous.GetFromJsonAsync<List<BrandDto>>("/api/brands", ApiFactory.Json);
        var response = await Admin.PostAsJsonAsync(
            $"/api/admin/brands/{brands![0].Id}/models", new CreateNameRequest("Model-" + Guid.NewGuid().ToString("N")[..8]));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ModelDto>(ApiFactory.Json))!.Id;
    }

    protected static SaveCarRequest NewCar(int modelId, decimal price = 15_000m, int year = 2018, int mileage = 120_000) => new(
        modelId, year, price, mileage, FuelType.Diesel, Transmission.Manual, BodyType.Sedan,
        EngineSizeCc: 1968, PowerHp: 150, Color: "Black", Doors: 5, Vin: "WVWZZZ3CZJE123456",
        IsCustomsCleared: true, DescriptionSq: "Përshkrim", DescriptionEn: "Description");

    protected async Task<int> CreateCarAsync(SaveCarRequest request)
    {
        var response = await Admin.PostAsJsonAsync("/api/admin/cars", request, ApiFactory.Json);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdResponse>(ApiFactory.Json))!.Id;
    }

    protected record IdResponse(int Id);
}
