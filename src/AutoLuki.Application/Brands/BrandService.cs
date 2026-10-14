using AutoLuki.Application.Common;
using AutoLuki.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AutoLuki.Application.Brands;

public record ModelDto(int Id, string Name);

public record BrandDto(int Id, string Name, IReadOnlyList<ModelDto> Models);

public record CreateNameRequest(string Name);

public class CreateNameRequestValidator : AbstractValidator<CreateNameRequest>
{
    public CreateNameRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
    }
}

public interface IBrandService
{
    Task<IReadOnlyList<BrandDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<BrandDto> CreateBrandAsync(CreateNameRequest request, CancellationToken cancellationToken = default);

    Task<ModelDto> CreateModelAsync(int brandId, CreateNameRequest request, CancellationToken cancellationToken = default);
}

public class BrandService(IAppDbContext db, IValidator<CreateNameRequest> validator) : IBrandService
{
    public async Task<IReadOnlyList<BrandDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Brands.AsNoTracking()
            .OrderBy(b => b.Name)
            .Select(b => new BrandDto(
                b.Id,
                b.Name,
                b.Models.OrderBy(m => m.Name).Select(m => new ModelDto(m.Id, m.Name)).ToList()))
            .ToListAsync(cancellationToken);

    public async Task<BrandDto> CreateBrandAsync(CreateNameRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var name = request.Name.Trim();

        if (await db.Brands.AnyAsync(b => b.Name.ToLower() == name.ToLower(), cancellationToken))
        {
            throw new BusinessRuleException($"Brand '{name}' already exists.");
        }

        var brand = new Brand { Name = name };
        db.Brands.Add(brand);
        await db.SaveChangesAsync(cancellationToken);
        return new BrandDto(brand.Id, brand.Name, []);
    }

    public async Task<ModelDto> CreateModelAsync(int brandId, CreateNameRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var name = request.Name.Trim();

        if (!await db.Brands.AnyAsync(b => b.Id == brandId, cancellationToken))
        {
            throw new NotFoundException(nameof(Brand), brandId);
        }

        if (await db.Models.AnyAsync(m => m.BrandId == brandId && m.Name.ToLower() == name.ToLower(), cancellationToken))
        {
            throw new BusinessRuleException($"Model '{name}' already exists for this brand.");
        }

        var model = new Model { BrandId = brandId, Name = name };
        db.Models.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        return new ModelDto(model.Id, model.Name);
    }
}
