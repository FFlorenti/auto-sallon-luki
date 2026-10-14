namespace AutoLuki.Domain;

public class Car : IAuditable
{
    public int Id { get; set; }
    public int ModelId { get; set; }
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int MileageKm { get; set; }
    public FuelType FuelType { get; set; }
    public Transmission Transmission { get; set; }
    public BodyType BodyType { get; set; }
    public int? EngineSizeCc { get; set; }
    public int? PowerHp { get; set; }
    public string? Color { get; set; }
    public int? Doors { get; set; }
    public string? Vin { get; set; }
    public bool IsCustomsCleared { get; set; }
    public string? DescriptionSq { get; set; }
    public string? DescriptionEn { get; set; }

    public CarStatus Status { get; private set; } = CarStatus.Available;
    public DateTimeOffset? SoldAt { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Model Model { get; set; } = null!;
    public ICollection<CarPhoto> Photos { get; set; } = [];

    public void MarkAsSold(DateTimeOffset now)
    {
        Status = CarStatus.Sold;
        SoldAt = now;
    }

    public void MarkAsAvailable()
    {
        Status = CarStatus.Available;
        SoldAt = null;
    }

    public void SoftDelete() => IsDeleted = true;
}
