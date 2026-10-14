using AutoLuki.Domain;

namespace AutoLuki.UnitTests.Domain;

public class CarTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 7, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void New_car_is_available()
    {
        var car = new Car();

        Assert.Equal(CarStatus.Available, car.Status);
        Assert.Null(car.SoldAt);
    }

    [Fact]
    public void MarkAsSold_sets_status_and_sold_date()
    {
        var car = new Car();

        car.MarkAsSold(Now);

        Assert.Equal(CarStatus.Sold, car.Status);
        Assert.Equal(Now, car.SoldAt);
    }

    [Fact]
    public void Making_a_sold_car_available_again_clears_the_sold_date()
    {
        var car = new Car();
        car.MarkAsSold(Now);

        car.MarkAsAvailable();

        Assert.Equal(CarStatus.Available, car.Status);
        Assert.Null(car.SoldAt);
    }

    [Fact]
    public void SoftDelete_marks_car_as_deleted()
    {
        var car = new Car();

        car.SoftDelete();

        Assert.True(car.IsDeleted);
    }
}
