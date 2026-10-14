using AutoLuki.Application.Cars;
using AutoLuki.Domain;
using Microsoft.Extensions.Time.Testing;

namespace AutoLuki.UnitTests.Cars;

public class SaveCarRequestValidatorTests
{
    private readonly SaveCarRequestValidator _validator =
        new(new FakeTimeProvider(new DateTimeOffset(2026, 10, 7, 0, 0, 0, TimeSpan.Zero)));

    private static SaveCarRequest ValidRequest() => new(
        ModelId: 1, Year: 2018, Price: 12_500m, MileageKm: 145_000,
        FuelType: FuelType.Diesel, Transmission: Transmission.Manual, BodyType: BodyType.Hatchback,
        EngineSizeCc: 1598, PowerHp: 115, Color: "Grey", Doors: 5, Vin: "WVWZZZAUZJW123456",
        IsCustomsCleared: true, DescriptionSq: null, DescriptionEn: null);

    [Fact]
    public void Valid_request_passes()
    {
        Assert.True(_validator.Validate(ValidRequest()).IsValid);
    }

    [Fact]
    public void Optional_fields_may_be_empty()
    {
        var request = ValidRequest() with { EngineSizeCc = null, PowerHp = null, Color = null, Doors = null, Vin = null };

        Assert.True(_validator.Validate(request).IsValid);
    }

    [Theory]
    [InlineData(1949)]
    [InlineData(2028)]
    public void Year_outside_range_fails(int year)
    {
        var result = _validator.Validate(ValidRequest() with { Year = year });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SaveCarRequest.Year));
    }

    [Fact]
    public void Next_years_model_is_allowed()
    {
        Assert.True(_validator.Validate(ValidRequest() with { Year = 2027 }).IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(12.345)]
    public void Invalid_price_fails(decimal price)
    {
        var result = _validator.Validate(ValidRequest() with { Price = price });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SaveCarRequest.Price));
    }

    [Fact]
    public void Negative_mileage_fails()
    {
        var result = _validator.Validate(ValidRequest() with { MileageKm = -1 });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SaveCarRequest.MileageKm));
    }

    [Theory]
    [InlineData("TOO-SHORT")]
    [InlineData("WVWZZZAUZJW12345O")]
    [InlineData("WVWZZZAUZJW1234567")]
    public void Invalid_vin_fails(string vin)
    {
        var result = _validator.Validate(ValidRequest() with { Vin = vin });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SaveCarRequest.Vin));
    }

    [Fact]
    public void Lowercase_vin_is_accepted()
    {
        Assert.True(_validator.Validate(ValidRequest() with { Vin = "wvwzzzauzjw123456" }).IsValid);
    }

    [Fact]
    public void Undefined_enum_value_fails()
    {
        var result = _validator.Validate(ValidRequest() with { FuelType = (FuelType)99 });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SaveCarRequest.FuelType));
    }
}
