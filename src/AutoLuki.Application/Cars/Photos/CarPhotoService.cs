using AutoLuki.Application.Common;
using AutoLuki.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoLuki.Application.Cars.Photos;

public record PhotoUpload(Stream Content, long Length);

public record ReorderPhotosRequest(IReadOnlyList<int> PhotoIds);

public interface ICarPhotoService
{
    Task<IReadOnlyList<CarPhotoDto>> UploadAsync(int carId, IReadOnlyList<PhotoUpload> uploads, CancellationToken cancellationToken = default);

    Task DeleteAsync(int carId, int photoId, CancellationToken cancellationToken = default);

    Task ReorderAsync(int carId, ReorderPhotosRequest request, CancellationToken cancellationToken = default);
}

public class CarPhotoService(
    IAppDbContext db,
    IFileStorage fileStorage,
    IImageProcessor imageProcessor,
    ILogger<CarPhotoService> logger) : ICarPhotoService
{
    public const int MaxPhotosPerCar = 30;
    public const long MaxFileSizeBytes = 10 * 1024 * 1024;

    public async Task<IReadOnlyList<CarPhotoDto>> UploadAsync(
        int carId, IReadOnlyList<PhotoUpload> uploads, CancellationToken cancellationToken = default)
    {
        if (uploads.Count == 0)
        {
            throw new BusinessRuleException("No photos were uploaded.");
        }

        await EnsureCarExistsAsync(carId, cancellationToken);

        var existing = await db.CarPhotos.Where(p => p.CarId == carId).Select(p => p.SortOrder).ToListAsync(cancellationToken);
        if (existing.Count + uploads.Count > MaxPhotosPerCar)
        {
            throw new BusinessRuleException($"A car can have at most {MaxPhotosPerCar} photos.");
        }

        var processed = new List<ProcessedImage>();
        foreach (var upload in uploads)
        {
            if (upload.Length is 0 or > MaxFileSizeBytes)
            {
                throw new BusinessRuleException($"Each photo must be between 1 byte and {MaxFileSizeBytes / 1024 / 1024} MB.");
            }

            const string notAnImage = "Only JPEG, PNG and WebP images are allowed.";
            _ = await ImageFormat.DetectAsync(upload.Content, cancellationToken) ?? throw new BusinessRuleException(notAnImage);
            processed.Add(await imageProcessor.ProcessAsync(upload.Content, cancellationToken)
                ?? throw new BusinessRuleException(notAnImage));
        }

        var nextOrder = existing.Count == 0 ? 0 : existing.Max() + 1;
        var photos = new List<CarPhoto>();
        foreach (var image in processed)
        {
            var name = $"cars/{carId}/{Guid.NewGuid():N}";
            var photo = new CarPhoto
            {
                CarId = carId,
                BlobName = name + image.Large.Extension,
                ThumbBlobName = name + "_thumb" + image.Thumbnail.Extension,
                SortOrder = nextOrder++,
            };

            await StoreAsync(image.Large, photo.BlobName, cancellationToken);
            await StoreAsync(image.Thumbnail, photo.ThumbBlobName, cancellationToken);
            photos.Add(photo);
        }

        db.CarPhotos.AddRange(photos);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("{Count} photos uploaded for car {CarId}", photos.Count, carId);
        return photos.Select(p => new CarPhotoDto(
            p.Id, fileStorage.GetPublicUrl(p.BlobName), fileStorage.GetPublicUrl(p.ThumbBlobName!), p.SortOrder)).ToList();
    }

    private async Task StoreAsync(ImageVariant variant, string blobName, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(variant.Content, writable: false);
        await fileStorage.UploadAsync(stream, blobName, variant.ContentType, cancellationToken);
    }

    public async Task DeleteAsync(int carId, int photoId, CancellationToken cancellationToken = default)
    {
        var photo = await db.CarPhotos.FirstOrDefaultAsync(p => p.Id == photoId && p.CarId == carId, cancellationToken)
            ?? throw new NotFoundException(nameof(CarPhoto), photoId);

        db.CarPhotos.Remove(photo);
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            await fileStorage.DeleteAsync(photo.BlobName, cancellationToken);
            if (photo.ThumbBlobName is not null)
            {
                await fileStorage.DeleteAsync(photo.ThumbBlobName, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not delete file {BlobName}", photo.BlobName);
        }
    }

    public async Task ReorderAsync(int carId, ReorderPhotosRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureCarExistsAsync(carId, cancellationToken);
        var photos = await db.CarPhotos.Where(p => p.CarId == carId).ToListAsync(cancellationToken);

        var sameSet = request.PhotoIds.Count == photos.Count
            && request.PhotoIds.Distinct().Count() == photos.Count
            && request.PhotoIds.All(id => photos.Any(p => p.Id == id));
        if (!sameSet)
        {
            throw new BusinessRuleException("The list must contain each of the car's photos exactly once.");
        }

        for (var i = 0; i < request.PhotoIds.Count; i++)
        {
            photos.Single(p => p.Id == request.PhotoIds[i]).SortOrder = i;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureCarExistsAsync(int carId, CancellationToken cancellationToken)
    {
        if (!await db.Cars.AnyAsync(c => c.Id == carId, cancellationToken))
        {
            throw new NotFoundException(nameof(Car), carId);
        }
    }
}
