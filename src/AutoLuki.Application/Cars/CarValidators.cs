using FluentValidation;

namespace AutoLuki.Application.Cars;

public class SaveCarRequestValidator : AbstractValidator<SaveCarRequest>
{
    public SaveCarRequestValidator(TimeProvider timeProvider)
    {
        var maxYear = timeProvider.GetUtcNow().Year + 1;

        RuleFor(x => x.ModelId).GreaterThan(0);
        RuleFor(x => x.Year).InclusiveBetween(1950, maxYear);
        RuleFor(x => x.Price).GreaterThan(0).LessThan(10_000_000).PrecisionScale(10, 2, ignoreTrailingZeros: true);
        RuleFor(x => x.MileageKm).InclusiveBetween(0, 2_000_000);
        RuleFor(x => x.FuelType).IsInEnum();
        RuleFor(x => x.Transmission).IsInEnum();
        RuleFor(x => x.BodyType).IsInEnum();
        RuleFor(x => x.EngineSizeCc).InclusiveBetween(50, 10_000).When(x => x.EngineSizeCc is not null);
        RuleFor(x => x.PowerHp).InclusiveBetween(1, 2_000).When(x => x.PowerHp is not null);
        RuleFor(x => x.Doors).InclusiveBetween(1, 6).When(x => x.Doors is not null);
        RuleFor(x => x.Color).MaximumLength(30);
        RuleFor(x => x.Vin).Length(17).Matches("^[A-HJ-NPR-Za-hj-npr-z0-9]{17}$")
            .WithMessage("VIN must be 17 characters (letters except I, O, Q, and digits).")
            .When(x => !string.IsNullOrEmpty(x.Vin));
        RuleFor(x => x.DescriptionSq).MaximumLength(4000);
        RuleFor(x => x.DescriptionEn).MaximumLength(4000);
    }
}

public class CarQueryValidator : AbstractValidator<CarQuery>
{
    public CarQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, CarQuery.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(100);
        RuleFor(x => x.YearTo).GreaterThanOrEqualTo(x => x.YearFrom)
            .When(x => x.YearFrom is not null && x.YearTo is not null);
        RuleFor(x => x.PriceTo).GreaterThanOrEqualTo(x => x.PriceFrom)
            .When(x => x.PriceFrom is not null && x.PriceTo is not null);
        RuleFor(x => x.Sort).IsInEnum();
    }
}
