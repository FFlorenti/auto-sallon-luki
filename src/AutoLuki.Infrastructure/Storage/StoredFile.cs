namespace AutoLuki.Infrastructure.Storage;

public class StoredFile
{
    public required string Name { get; set; }
    public required string ContentType { get; set; }
    public required byte[] Content { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
