using AutoLuki.Application.Common;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace AutoLuki.Infrastructure.Storage;

public class AzureBlobFileStorage : IFileStorage
{
    private readonly BlobContainerClient _container;
    private readonly string _publicBaseUrl;

    public AzureBlobFileStorage(IOptions<StorageOptions> options)
    {
        var o = options.Value;
        if (string.IsNullOrWhiteSpace(o.AzureConnectionString))
        {
            throw new InvalidOperationException("Storage:AzureConnectionString is required when Storage:Provider is 'Azure'.");
        }

        _container = new BlobContainerClient(o.AzureConnectionString, o.AzureContainerName);
        _publicBaseUrl = (o.PublicBaseUrl ?? _container.Uri.ToString()).TrimEnd('/');
    }

    public async Task UploadAsync(Stream content, string blobName, string contentType, CancellationToken cancellationToken = default)
    {
        await _container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

        await _container.GetBlobClient(blobName).UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType,
                CacheControl = "public, max-age=31536000, immutable",
            },
        }, cancellationToken);
    }

    public async Task DeleteAsync(string blobName, CancellationToken cancellationToken = default) =>
        await _container.GetBlobClient(blobName).DeleteIfExistsAsync(cancellationToken: cancellationToken);

    public string GetPublicUrl(string blobName) => $"{_publicBaseUrl}/{blobName}";
}
