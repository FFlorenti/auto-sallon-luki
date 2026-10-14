namespace AutoLuki.Application.Common;

public interface IFileStorage
{
    Task UploadAsync(Stream content, string blobName, string contentType, CancellationToken cancellationToken = default);

    Task DeleteAsync(string blobName, CancellationToken cancellationToken = default);

    string GetPublicUrl(string blobName);
}
