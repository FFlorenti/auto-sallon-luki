using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AutoLuki.Application.Cars;
using AutoLuki.IntegrationTests.Infrastructure;
using SkiaSharp;

namespace AutoLuki.IntegrationTests;

public class DatabaseStorageTests(PostgresFixture postgres)
    : IntegrationTestBase(postgres, new Dictionary<string, string?> { ["Storage:Provider"] = "Database" })
{
    private async Task<CarPhotoDto> UploadPhotoAsync(int carId)
    {
        var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(TestImages.Jpeg(2000, 1500));
        file.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        form.Add(file, "files", "photo.jpg");

        var response = await Admin.PostAsync($"/api/admin/cars/{carId}/photos", form);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<CarPhotoDto>>(ApiFactory.Json))![0];
    }

    [Fact]
    public async Task Photos_are_served_from_the_database_with_long_lived_caching()
    {
        var id = await CreateCarAsync(NewCar(await CreateUniqueModelAsync()));

        var photo = await UploadPhotoAsync(id);

        var large = await Anonymous.GetAsync(photo.Url);
        var thumb = await Anonymous.GetAsync(photo.ThumbUrl);
        Assert.Equal(HttpStatusCode.OK, large.StatusCode);
        Assert.Equal("image/jpeg", large.Content.Headers.ContentType?.MediaType);
        Assert.Equal("image/webp", thumb.Content.Headers.ContentType?.MediaType);
        Assert.Contains("immutable", large.Headers.CacheControl?.ToString());
        Assert.Equal((1600, 1200, SKEncodedImageFormat.Jpeg), TestImages.Describe(await large.Content.ReadAsByteArrayAsync()));
    }

    [Fact]
    public async Task Photos_survive_an_application_restart()
    {
        var id = await CreateCarAsync(NewCar(await CreateUniqueModelAsync()));
        var photo = await UploadPhotoAsync(id);

        await using var restarted = new ApiFactory(
            Postgres, "autoluki_" + nameof(DatabaseStorageTests).ToLowerInvariant(),
            new Dictionary<string, string?> { ["Storage:Provider"] = "Database" });

        Assert.Equal(HttpStatusCode.OK, (await restarted.CreateClient().GetAsync(photo.Url)).StatusCode);
    }

    [Fact]
    public async Task Deleting_a_photo_removes_it_from_the_database()
    {
        var id = await CreateCarAsync(NewCar(await CreateUniqueModelAsync()));
        var photo = await UploadPhotoAsync(id);

        await Admin.DeleteAsync($"/api/admin/cars/{id}/photos/{photo.Id}");

        Assert.Equal(HttpStatusCode.NotFound, (await Anonymous.GetAsync(photo.Url)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Anonymous.GetAsync(photo.ThumbUrl)).StatusCode);
    }

    [Fact]
    public async Task Unknown_file_returns_404()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await Anonymous.GetAsync("/uploads/cars/1/missing.jpg")).StatusCode);
    }
}
