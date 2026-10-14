using AutoLuki.Application.Common;
using AutoLuki.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoLuki.Application.Cars;

public class CarService(
    IAppDbContext db,
    IFileStorage fileStorage,
    IValidator<SaveCarRequest> saveValidator,
    IValidator<CarQuery> queryValidator,
    TimeProvider timeProvider,
    ILogger<CarService> logger) : ICarService
{
    public Task<PagedResult<CarListItemDto>> SearchPublicAsync(CarQuery query, CancellationToken cancellationToken = default) =>
        SearchAsync(query, isAdmin: false, cancellationToken);

    public Task<PagedResult<CarListItemDto>> SearchAdminAsync(CarQuery query, CancellationToken cancellationToken = default) =>
        SearchAsync(query, isAdmin: true, cancellationToken);

    public async Task<CarDetailDto> GetPublicAsync(int id, CancellationToken cancellationToken = default)
    {
        var detail = await ProjectDetail(db.Cars.AsNoTracking().Where(c => c.Id == id))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Car), id);

        return WithPhotoUrls(detail);
    }

    public async Task<AdminCarDetailDto> GetAdminAsync(int id, CancellationToken cancellationToken = default)
    {
        var detail = await GetPublicAsync(id, cancellationToken);
        var extra = await db.Cars.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new { c.Vin, c.SoldAt, c.CreatedAt, c.UpdatedAt })
            .FirstAsync(cancellationToken);

        return new AdminCarDetailDto(detail, extra.Vin, extra.SoldAt, extra.CreatedAt, extra.UpdatedAt);
    }

    public async Task<int> CreateAsync(SaveCarRequest request, CancellationToken cancellationToken = default)
    {
        await saveValidator.ValidateAndThrowAsync(request, cancellationToken);
        await EnsureModelExistsAsync(request.ModelId, cancellationToken);

        var car = new Car();
        Apply(car, request);
        db.Cars.Add(car);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Car {CarId} created", car.Id);
        return car.Id;
    }

    public async Task UpdateAsync(int id, SaveCarRequest request, CancellationToken cancellationToken = default)
    {
        await saveValidator.ValidateAndThrowAsync(request, cancellationToken);
        var car = await FindAsync(id, cancellationToken);
        if (car.ModelId != request.ModelId)
        {
            await EnsureModelExistsAsync(request.ModelId, cancellationToken);
        }

        Apply(car, request);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangeStatusAsync(int id, ChangeCarStatusRequest request, CancellationToken cancellationToken = default)
    {
        var car = await FindAsync(id, cancellationToken);

        switch (request.Status)
        {
            case CarStatus.Sold:
                car.MarkAsSold(timeProvider.GetUtcNow());
                break;
            case CarStatus.Available:
                car.MarkAsAvailable();
                break;
            default:
                throw new BusinessRuleException($"Unknown status '{request.Status}'.");
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Car {CarId} status changed to {Status}", id, request.Status);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var car = await FindAsync(id, cancellationToken);
        car.SoftDelete();
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Car {CarId} deleted", id);
    }

    private async Task<PagedResult<CarListItemDto>> SearchAsync(CarQuery query, bool isAdmin, CancellationToken cancellationToken)
    {
        await queryValidator.ValidateAndThrowAsync(query, cancellationToken);

        var cars = ApplyFilters(db.Cars.AsNoTracking(), query, isAdmin);
        var totalCount = await cars.CountAsync(cancellationToken);

        var items = await ApplySort(cars, query.Sort)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new CarListItemDto(
                c.Id,
                c.Model.Brand.Name,
                c.Model.Name,
                c.Year,
                c.Price,
                c.MileageKm,
                c.FuelType,
                c.Transmission,
                c.BodyType,
                c.Status,
                c.Photos.OrderBy(p => p.SortOrder).Select(p => p.ThumbBlobName ?? p.BlobName).FirstOrDefault()))
            .ToListAsync(cancellationToken);

        var withUrls = items
            .Select(i => i with { CoverPhotoUrl = i.CoverPhotoUrl is null ? null : fileStorage.GetPublicUrl(i.CoverPhotoUrl) })
            .ToList();

        return new PagedResult<CarListItemDto>(withUrls, query.Page, query.PageSize, totalCount);
    }

    private static IQueryable<Car> ApplyFilters(IQueryable<Car> cars, CarQuery q, bool isAdmin)
    {
        if (isAdmin)
        {
            if (q.Status is not null) cars = cars.Where(c => c.Status == q.Status);
        }
        else
        {
            cars = cars.Where(c => c.Status != CarStatus.Sold);
        }

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim().ToLower();
            cars = cars.Where(c =>
                (c.Model.Brand.Name + " " + c.Model.Name).ToLower().Contains(term));
        }

        if (q.BrandId is not null) cars = cars.Where(c => c.Model.BrandId == q.BrandId);
        if (q.ModelId is not null) cars = cars.Where(c => c.ModelId == q.ModelId);
        if (q.YearFrom is not null) cars = cars.Where(c => c.Year >= q.YearFrom);
        if (q.YearTo is not null) cars = cars.Where(c => c.Year <= q.YearTo);
        if (q.PriceFrom is not null) cars = cars.Where(c => c.Price >= q.PriceFrom);
        if (q.PriceTo is not null) cars = cars.Where(c => c.Price <= q.PriceTo);
        if (q.MileageMax is not null) cars = cars.Where(c => c.MileageKm <= q.MileageMax);
        if (q.FuelType is not null) cars = cars.Where(c => c.FuelType == q.FuelType);
        if (q.Transmission is not null) cars = cars.Where(c => c.Transmission == q.Transmission);
        if (q.BodyType is not null) cars = cars.Where(c => c.BodyType == q.BodyType);

        return cars;
    }

    private static IQueryable<Car> ApplySort(IQueryable<Car> cars, CarSort sort) => sort switch
    {
        CarSort.PriceAsc => cars.OrderBy(c => c.Price).ThenByDescending(c => c.Id),
        CarSort.PriceDesc => cars.OrderByDescending(c => c.Price).ThenByDescending(c => c.Id),
        CarSort.YearDesc => cars.OrderByDescending(c => c.Year).ThenByDescending(c => c.Id),
        CarSort.MileageAsc => cars.OrderBy(c => c.MileageKm).ThenByDescending(c => c.Id),
        _ => cars.OrderByDescending(c => c.CreatedAt).ThenByDescending(c => c.Id),
    };

    private static IQueryable<CarDetailDto> ProjectDetail(IQueryable<Car> cars) =>
        cars.Select(c => new CarDetailDto(
            c.Id,
            c.Model.BrandId,
            c.Model.Brand.Name,
            c.ModelId,
            c.Model.Name,
            c.Year,
            c.Price,
            c.MileageKm,
            c.FuelType,
            c.Transmission,
            c.BodyType,
            c.EngineSizeCc,
            c.PowerHp,
            c.Color,
            c.Doors,
            c.IsCustomsCleared,
            c.DescriptionSq,
            c.DescriptionEn,
            c.Status,
            c.Photos.OrderBy(p => p.SortOrder)
                .Select(p => new CarPhotoDto(p.Id, p.BlobName, p.ThumbBlobName ?? p.BlobName, p.SortOrder))
                .ToList()));

    private CarDetailDto WithPhotoUrls(CarDetailDto detail) => detail with
    {
        Photos = detail.Photos
            .Select(p => p with { Url = fileStorage.GetPublicUrl(p.Url), ThumbUrl = fileStorage.GetPublicUrl(p.ThumbUrl) })
            .ToList(),
    };

    private async Task<Car> FindAsync(int id, CancellationToken cancellationToken) =>
        await db.Cars.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
        ?? throw new NotFoundException(nameof(Car), id);

    private async Task EnsureModelExistsAsync(int modelId, CancellationToken cancellationToken)
    {
        if (!await db.Models.AnyAsync(m => m.Id == modelId, cancellationToken))
        {
            throw new BusinessRuleException($"Model with id '{modelId}' does not exist.");
        }
    }

    private static void Apply(Car car, SaveCarRequest r)
    {
        car.ModelId = r.ModelId;
        car.Year = r.Year;
        car.Price = r.Price;
        car.MileageKm = r.MileageKm;
        car.FuelType = r.FuelType;
        car.Transmission = r.Transmission;
        car.BodyType = r.BodyType;
        car.EngineSizeCc = r.EngineSizeCc;
        car.PowerHp = r.PowerHp;
        car.Color = Normalize(r.Color);
        car.Doors = r.Doors;
        car.Vin = Normalize(r.Vin)?.ToUpperInvariant();
        car.IsCustomsCleared = r.IsCustomsCleared;
        car.DescriptionSq = Normalize(r.DescriptionSq);
        car.DescriptionEn = Normalize(r.DescriptionEn);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
