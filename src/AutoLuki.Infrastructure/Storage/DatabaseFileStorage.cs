using AutoLuki.Application.Common;
using AutoLuki.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoLuki.Infrastructure.Storage;

public class DatabaseFileStorage(AppDbContext db, TimeProvider timeProvider) : IFileStorage
{
    public const string RequestPath = "/uploads";

    public async Task UploadAsync(Stream content, string blobName, string contentType, CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);

        await db.StoredFiles.Where(f => f.Name == blobName).ExecuteDeleteAsync(cancellationToken);
        db.StoredFiles.Add(new StoredFile
        {
            Name = blobName,
            ContentType = contentType,
            Content = buffer.ToArray(),
            CreatedAt = timeProvider.GetUtcNow(),
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task DeleteAsync(string blobName, CancellationToken cancellationToken = default) =>
        db.StoredFiles.Where(f => f.Name == blobName).ExecuteDeleteAsync(cancellationToken);

    public string GetPublicUrl(string blobName) => $"{RequestPath}/{blobName}";

    public Task<StoredFile?> FindAsync(string blobName, CancellationToken cancellationToken = default) =>
        db.StoredFiles.AsNoTracking().FirstOrDefaultAsync(f => f.Name == blobName, cancellationToken);
}
