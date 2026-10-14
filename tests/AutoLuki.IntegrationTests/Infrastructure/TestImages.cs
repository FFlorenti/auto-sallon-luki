using SkiaSharp;

namespace AutoLuki.IntegrationTests.Infrastructure;

public static class TestImages
{
    public static byte[] Jpeg(int width = 800, int height = 600) => Create(width, height, SKEncodedImageFormat.Jpeg);

    public static byte[] Png(int width = 800, int height = 600) => Create(width, height, SKEncodedImageFormat.Png);

    public static byte[] FakeJpeg() => [0xFF, 0xD8, 0xFF, 0xE0, .. "this is definitely not image data"u8];

    public static (int Width, int Height, SKEncodedImageFormat Format) Describe(byte[] image)
    {
        using var codec = SKCodec.Create(new MemoryStream(image));
        return (codec.Info.Width, codec.Info.Height, codec.EncodedFormat);
    }

    private static byte[] Create(int width, int height, SKEncodedImageFormat format)
    {
        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
        {
            using var paint = new SKPaint();
            paint.Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0), new SKPoint(width, height), [SKColors.DarkBlue, SKColors.Orange], SKShaderTileMode.Clamp);
            canvas.DrawRect(0, 0, width, height, paint);
            paint.Shader = null;
            paint.Color = SKColors.White;
            canvas.DrawCircle(width / 3f, height / 2f, Math.Min(width, height) / 5f, paint);
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, 90);
        return data.ToArray();
    }
}
