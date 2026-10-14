using AutoLuki.Application.Cars;

namespace AutoLuki.UnitTests.Cars;

public class CarQueryValidatorTests
{
    private readonly CarQueryValidator _validator = new();

    [Fact]
    public void Default_query_is_valid()
    {
        Assert.True(_validator.Validate(new CarQuery()).IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(CarQuery.MaxPageSize + 1)]
    public void Page_size_outside_limits_fails(int pageSize)
    {
        Assert.False(_validator.Validate(new CarQuery { PageSize = pageSize }).IsValid);
    }

    [Fact]
    public void Page_must_start_at_one()
    {
        Assert.False(_validator.Validate(new CarQuery { Page = 0 }).IsValid);
    }

    [Fact]
    public void Reversed_price_range_fails()
    {
        Assert.False(_validator.Validate(new CarQuery { PriceFrom = 20_000, PriceTo = 10_000 }).IsValid);
    }

    [Fact]
    public void Reversed_year_range_fails()
    {
        Assert.False(_validator.Validate(new CarQuery { YearFrom = 2020, YearTo = 2015 }).IsValid);
    }

    [Fact]
    public void One_sided_ranges_are_valid()
    {
        Assert.True(_validator.Validate(new CarQuery { PriceTo = 10_000, YearFrom = 2015 }).IsValid);
    }
}
