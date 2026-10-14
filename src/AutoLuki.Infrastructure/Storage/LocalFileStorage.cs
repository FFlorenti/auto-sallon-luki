using AutoLuki.Application.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AutoLuki.Infrastructure.Storage;

public class LocalFileStorage(IOptions<StorageOptions> options, IHostEnvironment environment) : IFileStorage
{
    public const string RequestPath = "/uploads";

    private readonly string _root = Path.GetFullPath(Path.Combine(environment.ContentRootPath, options.Value.LocalPath));
    private readonly string _publicBaseUrl = (options.Value.PublicBaseUrl ?? RequestPath).TrimEnd('/');

    public string RootPath => _root;

    public async Task UploadAsync(Stream content, string blobName, string contentType, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(blobName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var file = File.Create(path);
        await content.CopyToAsync(file, cancellationToken);
    }

    public Task DeleteAsync(string blobName, CancellationToken cancellationToken = default)
    {
        File.Delete(ResolvePath(blobName));
        return Task.CompletedTask;
    }

    public string GetPublicUrl(string blobName) => $"{_publicBaseUrl}/{blobName}";

    private string ResolvePath(string blobName)
    {
        var path = Path.GetFullPath(Path.Combine(_root, blobName));

        if (!path.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid blob name.");
        }

        return path;
    }
}
