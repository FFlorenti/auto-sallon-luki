using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AutoLuki.Application.Cars;
using AutoLuki.Application.Cars.Photos;
using AutoLuki.IntegrationTests.Infrastructure;
using SkiaSharp;

namespace AutoLuki.IntegrationTests;

public class PhotosTests(PostgresFixture postgres) : IntegrationTestBase(postgres)
{
    private static readonly byte[] Photo = TestImages.Jpeg();

    private static MultipartFormDataContent Files(params (byte[] Bytes, string Name)[] files)
    {
        var form = new MultipartFormDataContent();
        foreach (var (bytes, name) in files)
        {
            var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            form.Add(content, "files", name);
        }
        return form;
    }

    private async Task<List<CarPhotoDto>> UploadAsync(int carId, params (byte[] Bytes, string Name)[] files)
    {
        var response = await Admin.PostAsync($"/api/admin/cars/{carId}/photos", Files(files));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<CarPhotoDto>>(ApiFactory.Json))!;
    }

    [Fact]
    public async Task Uploaded_photos_are_stored_ordered_and_served()
    {
        var id = await CreateCarAsync(NewCar(await CreateUniqueModelAsync()));

        var photos = await UploadAsync(id, (Photo, "a.jpg"), (Photo, "b.jpg"));

        Assert.Equal([0, 1], photos.Select(p => p.SortOrder));
        Assert.All(photos, p => Assert.StartsWith($"/uploads/cars/{id}/", p.Url));
        Assert.Equal(HttpStatusCode.OK, (await Anonymous.GetAsync(photos[0].Url)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Anonymous.GetAsync(photos[0].ThumbUrl)).StatusCode);
    }

    [Fact]
    public async Task Large_phone_photo_is_resized_to_a_gallery_image_and_a_thumbnail()
    {
        var id = await CreateCarAsync(NewCar(await CreateUniqueModelAsync()));
        var phonePhoto = TestImages.Jpeg(4000, 3000);

        var photo = (await UploadAsync(id, (phonePhoto, "IMG_0001.jpg")))[0];

        var large = await Anonymous.GetByteArrayAsync(photo.Url);
        var thumb = await Anonymous.GetByteArrayAsync(photo.ThumbUrl);
        Assert.Equal((1600, 1200, SKEncodedImageFormat.Jpeg), TestImages.Describe(large));
        Assert.Equal((640, 480, SKEncodedImageFormat.Webp), TestImages.Describe(thumb));
        Assert.True(large.Length < phonePhoto.Length);
        Assert.True(thumb.Length < large.Length);
    }

    [Fact]
    public async Task Small_photo_is_not_enlarged_and_png_becomes_jpeg()
    {
        var id = await CreateCarAsync(NewCar(await CreateUniqueModelAsync()));

        var photo = (await UploadAsync(id, (TestImages.Png(500, 400), "small.png")))[0];

        Assert.EndsWith(".jpg", photo.Url);
        Assert.Equal((500, 400, SKEncodedImageFormat.Jpeg), TestImages.Describe(await Anonymous.GetByteArrayAsync(photo.Url)));
    }

    [Fact]
    public async Task The_car_list_shows_the_thumbnail_of_the_first_photo()
    {
        var modelId = await CreateUniqueModelAsync();
        var id = await CreateCarAsync(NewCar(modelId));
        var photos = await UploadAsync(id, (Photo, "a.jpg"), (Photo, "b.jpg"));

        var list = await Anonymous.GetFromJsonAsync<AutoLuki.Application.Common.PagedResult<CarListItemDto>>(
            $"/api/cars?modelId={modelId}", ApiFactory.Json);

        Assert.Equal(photos[0].ThumbUrl, list!.Items.Single().CoverPhotoUrl);
    }

    [Theory]
    [InlineData("executable")]
    [InlineData("fake-jpeg")]
    public async Task Files_that_are_not_real_images_are_rejected(string kind)
    {
        var id = await CreateCarAsync(NewCar(await CreateUniqueModelAsync()));
        var bytes = kind == "executable" ? "MZ fake exe"u8.ToArray() : TestImages.FakeJpeg();

        var response = await Admin.PostAsync($"/api/admin/cars/{id}/photos", Files((bytes, "photo.jpg")));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var car = await Admin.GetFromJsonAsync<AdminCarDetailDto>($"/api/admin/cars/{id}", ApiFactory.Json);
        Assert.Empty(car!.Car.Photos);
    }

    [Fact]
    public async Task One_bad_file_rejects_the_whole_upload()
    {
        var id = await CreateCarAsync(NewCar(await CreateUniqueModelAsync()));

        var response = await Admin.PostAsync($"/api/admin/cars/{id}/photos", Files((Photo, "good.jpg"), (TestImages.FakeJpeg(), "bad.jpg")));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var car = await Admin.GetFromJsonAsync<AdminCarDetailDto>($"/api/admin/cars/{id}", ApiFactory.Json);
        Assert.Empty(car!.Car.Photos);
    }

    [Fact]
    public async Task Reorder_changes_the_cover_photo()
    {
        var id = await CreateCarAsync(NewCar(await CreateUniqueModelAsync()));
        var photos = await UploadAsync(id, (Photo, "a.jpg"), (Photo, "b.jpg"));

        var reorder = await Admin.PutAsJsonAsync($"/api/admin/cars/{id}/photos/order", new ReorderPhotosRequest([photos[1].Id, photos[0].Id]));

        Assert.Equal(HttpStatusCode.NoContent, reorder.StatusCode);
        var car = await Anonymous.GetFromJsonAsync<CarDetailDto>($"/api/cars/{id}", ApiFactory.Json);
        Assert.Equal(photos[1].Id, car!.Photos[0].Id);
    }

    [Fact]
    public async Task Reorder_with_an_incomplete_list_is_rejected()
    {
        var id = await CreateCarAsync(NewCar(await CreateUniqueModelAsync()));
        var photos = await UploadAsync(id, (Photo, "a.jpg"), (Photo, "b.jpg"));

        var reorder = await Admin.PutAsJsonAsync($"/api/admin/cars/{id}/photos/order", new ReorderPhotosRequest([photos[0].Id]));

        Assert.Equal(HttpStatusCode.BadRequest, reorder.StatusCode);
    }

    [Fact]
    public async Task Deleting_a_photo_removes_both_of_its_files()
    {
        var id = await CreateCarAsync(NewCar(await CreateUniqueModelAsync()));
        var photo = (await UploadAsync(id, (Photo, "a.jpg")))[0];

        var delete = await Admin.DeleteAsync($"/api/admin/cars/{id}/photos/{photo.Id}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Anonymous.GetAsync(photo.Url)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Anonymous.GetAsync(photo.ThumbUrl)).StatusCode);
    }
}
