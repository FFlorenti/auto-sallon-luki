namespace AutoLuki.Infrastructure.Storage;

public class StorageOptions
{
    public const string SectionName = "Storage";

    public string Provider { get; init; } = "Local";

    public string LocalPath { get; init; } = "uploads";

    public string? AzureConnectionString { get; init; }

    public string AzureContainerName { get; init; } = "car-photos";

    public string? PublicBaseUrl { get; init; }
}
