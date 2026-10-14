namespace AutoLuki.Application.Common;

public sealed record ImageVariant(byte[] Content, string ContentType, string Extension);

public sealed record ProcessedImage(ImageVariant Large, ImageVariant Thumbnail);

public interface IImageProcessor
{
    Task<ProcessedImage?> ProcessAsync(Stream source, CancellationToken cancellationToken = default);
}
