namespace AutoLuki.Domain;

public class Model
{
    public int Id { get; set; }
    public int BrandId { get; set; }
    public required string Name { get; set; }

    public Brand Brand { get; set; } = null!;
}
