using AutoLuki.Application.Common;
using SkiaSharp;

namespace AutoLuki.Infrastructure.Storage;

public class SkiaImageProcessor : IImageProcessor
{
    public const int LargeMaxSide = 1600;
    public const int ThumbnailMaxSide = 640;

    private const long MaxPixels = 60_000_000;

    private static readonly SKSamplingOptions Sampling = new(SKFilterMode.Linear, SKMipmapMode.Linear);

    public Task<ProcessedImage?> ProcessAsync(Stream source, CancellationToken cancellationToken = default) =>
        Task.Run(() => Process(source), cancellationToken);

    private static ProcessedImage? Process(Stream source)
    {
        using var data = SKData.Create(source);
        using var codec = SKCodec.Create(data);
        if (codec is null)
        {
            return null;
        }

        var info = codec.Info;
        if (info.Width <= 0 || info.Height <= 0 || (long)info.Width * info.Height > MaxPixels)
        {
            return null;
        }

        using var decoded = SKBitmap.Decode(codec);
        if (decoded is null)
        {
            return null;
        }

        using var upright = Normalize(decoded, codec.EncodedOrigin);

        return new ProcessedImage(
            Encode(upright, LargeMaxSide, SKEncodedImageFormat.Jpeg, quality: 82, "image/jpeg", ".jpg"),
            Encode(upright, ThumbnailMaxSide, SKEncodedImageFormat.Webp, quality: 78, "image/webp", ".webp"));
    }

    private static SKBitmap Normalize(SKBitmap source, SKEncodedOrigin origin)
    {
        var swapsSides = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop
            or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        var width = swapsSides ? source.Height : source.Width;
        var height = swapsSides ? source.Width : source.Height;

        var result = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque));
        using var canvas = new SKCanvas(result);
        canvas.Clear(SKColors.White);

        switch (origin)
        {
            case SKEncodedOrigin.TopRight:
                canvas.Translate(width, 0);
                canvas.Scale(-1, 1);
                break;
            case SKEncodedOrigin.BottomRight:
                canvas.Translate(width, height);
                canvas.RotateDegrees(180);
                break;
            case SKEncodedOrigin.BottomLeft:
                canvas.Translate(0, height);
                canvas.Scale(1, -1);
                break;
            case SKEncodedOrigin.LeftTop:
                canvas.RotateDegrees(90);
                canvas.Scale(1, -1);
                break;
            case SKEncodedOrigin.RightTop:
                canvas.Translate(width, 0);
                canvas.RotateDegrees(90);
                break;
            case SKEncodedOrigin.RightBottom:
                canvas.Translate(width, height);
                canvas.RotateDegrees(90);
                canvas.Scale(-1, 1);
                break;
            case SKEncodedOrigin.LeftBottom:
                canvas.Translate(0, height);
                canvas.RotateDegrees(270);
                break;
        }

        using var image = SKImage.FromBitmap(source);
        canvas.DrawImage(image, 0, 0, Sampling);
        return result;
    }

    private static ImageVariant Encode(
        SKBitmap source, int maxSide, SKEncodedImageFormat format, int quality, string contentType, string extension)
    {
        var scale = Math.Min(1.0, (double)maxSide / Math.Max(source.Width, source.Height));
        var size = new SKSizeI(
            Math.Max(1, (int)Math.Round(source.Width * scale)),
            Math.Max(1, (int)Math.Round(source.Height * scale)));

        using var resized = scale < 1.0 ? source.Resize(size, Sampling) : null;
        using var image = SKImage.FromBitmap(resized ?? source);
        using var encoded = image.Encode(format, quality);

        return new ImageVariant(encoded.ToArray(), contentType, extension);
    }
}
