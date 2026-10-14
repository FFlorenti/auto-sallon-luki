namespace AutoLuki.Domain;

public class CarPhoto : IHasCreatedAt
{
    public int Id { get; set; }
    public int CarId { get; set; }

    public required string BlobName { get; set; }

    public string? ThumbBlobName { get; set; }

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Car Car { get; set; } = null!;
}
