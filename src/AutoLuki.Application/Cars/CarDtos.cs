using AutoLuki.Domain;

namespace AutoLuki.Application.Cars;

public record CarListItemDto(
    int Id,
    string Brand,
    string Model,
    int Year,
    decimal Price,
    int MileageKm,
    FuelType FuelType,
    Transmission Transmission,
    BodyType BodyType,
    CarStatus Status,
    string? CoverPhotoUrl);

public record CarPhotoDto(int Id, string Url, string ThumbUrl, int SortOrder);

public record CarDetailDto(
    int Id,
    int BrandId,
    string Brand,
    int ModelId,
    string Model,
    int Year,
    decimal Price,
    int MileageKm,
    FuelType FuelType,
    Transmission Transmission,
    BodyType BodyType,
    int? EngineSizeCc,
    int? PowerHp,
    string? Color,
    int? Doors,
    bool IsCustomsCleared,
    string? DescriptionSq,
    string? DescriptionEn,
    CarStatus Status,
    IReadOnlyList<CarPhotoDto> Photos);

public record AdminCarDetailDto(
    CarDetailDto Car,
    string? Vin,
    DateTimeOffset? SoldAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record SaveCarRequest(
    int ModelId,
    int Year,
    decimal Price,
    int MileageKm,
    FuelType FuelType,
    Transmission Transmission,
    BodyType BodyType,
    int? EngineSizeCc,
    int? PowerHp,
    string? Color,
    int? Doors,
    string? Vin,
    bool IsCustomsCleared,
    string? DescriptionSq,
    string? DescriptionEn);

public record ChangeCarStatusRequest(CarStatus Status);
