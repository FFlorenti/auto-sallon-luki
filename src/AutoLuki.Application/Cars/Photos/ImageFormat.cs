namespace AutoLuki.Application.Cars.Photos;

public sealed record ImageFormat(string ContentType, string Extension)
{
    public static readonly ImageFormat Jpeg = new("image/jpeg", ".jpg");
    public static readonly ImageFormat Png = new("image/png", ".png");
    public static readonly ImageFormat Webp = new("image/webp", ".webp");

    public static async Task<ImageFormat?> DetectAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var header = new byte[12];
        var read = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, cancellationToken);
        stream.Position = 0;

        return Detect(header.AsSpan(0, read));
    }

    public static ImageFormat? Detect(ReadOnlySpan<byte> header)
    {
        if (header.StartsWith((ReadOnlySpan<byte>)[0xFF, 0xD8, 0xFF]))
        {
            return Jpeg;
        }

        if (header.StartsWith((ReadOnlySpan<byte>)[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]))
        {
            return Png;
        }

        if (header.Length >= 12 && header[..4].SequenceEqual("RIFF"u8) && header[8..12].SequenceEqual("WEBP"u8))
        {
            return Webp;
        }

        return null;
    }
}
