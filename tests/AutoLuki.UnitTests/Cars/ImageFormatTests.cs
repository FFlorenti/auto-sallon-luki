using System.Text;
using AutoLuki.Application.Cars.Photos;

namespace AutoLuki.UnitTests.Cars;

public class ImageFormatTests
{
    [Fact]
    public void Detects_jpeg()
    {
        Assert.Equal(ImageFormat.Jpeg, ImageFormat.Detect([0xFF, 0xD8, 0xFF, 0xE0, 0x00]));
    }

    [Fact]
    public void Detects_png()
    {
        Assert.Equal(ImageFormat.Png, ImageFormat.Detect([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00]));
    }

    [Fact]
    public void Detects_webp()
    {
        Assert.Equal(ImageFormat.Webp, ImageFormat.Detect(Encoding.ASCII.GetBytes("RIFF\0\0\0\0WEBPVP8 ")));
    }

    [Theory]
    [InlineData("MZ executable")]
    [InlineData("GIF89a")]
    [InlineData("<svg xmlns=")]
    [InlineData("")]
    public void Rejects_other_files(string content)
    {
        Assert.Null(ImageFormat.Detect(Encoding.ASCII.GetBytes(content)));
    }

    [Fact]
    public async Task DetectAsync_rewinds_the_stream_for_the_upload()
    {
        using var stream = new MemoryStream([0xFF, 0xD8, 0xFF, 0xE0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);

        var format = await ImageFormat.DetectAsync(stream);

        Assert.Equal(ImageFormat.Jpeg, format);
        Assert.Equal(0, stream.Position);
    }
}
