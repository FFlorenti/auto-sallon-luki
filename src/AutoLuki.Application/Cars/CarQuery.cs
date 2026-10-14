using AutoLuki.Domain;

namespace AutoLuki.Application.Cars;

public enum CarSort { Newest, PriceAsc, PriceDesc, YearDesc, MileageAsc }

public record CarQuery
{
    public const int MaxPageSize = 50;

    public string? Search { get; init; }
    public int? BrandId { get; init; }
    public int? ModelId { get; init; }
    public int? YearFrom { get; init; }
    public int? YearTo { get; init; }
    public decimal? PriceFrom { get; init; }
    public decimal? PriceTo { get; init; }
    public int? MileageMax { get; init; }
    public FuelType? FuelType { get; init; }
    public Transmission? Transmission { get; init; }
    public BodyType? BodyType { get; init; }

    public CarStatus? Status { get; init; }

    public CarSort Sort { get; init; } = CarSort.Newest;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 12;
}
