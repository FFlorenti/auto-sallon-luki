using System.Net;
using System.Net.Http.Headers;
using AutoLuki.IntegrationTests.Infrastructure;

namespace AutoLuki.IntegrationTests;

public class ShareTests(PostgresFixture postgres) : IntegrationTestBase(postgres)
{
    [Fact]
    public async Task Preview_page_has_title_price_and_canonical_link()
    {
        var id = await CreateCarAsync(NewCar(await CreateUniqueModelAsync(), price: 21_900m, year: 2018, mileage: 172_000));

        var response = await Anonymous.GetAsync($"/api/share/cars/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("2018 · 21 900 €\">", html);
        Assert.Contains("2018 · 172 000 km · Naftë · Manual", html);
        Assert.Contains($"<meta property=\"og:url\" content=\"http://localhost/cars/{id}\">", html);
        Assert.Contains("<meta name=\"twitter:card\" content=\"summary\">", html);
        Assert.DoesNotContain("og:image", html);
    }

    [Fact]
    public async Task Preview_uses_the_cover_photo_with_an_absolute_url()
    {
        var id = await CreateCarAsync(NewCar(await CreateUniqueModelAsync()));
        var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(TestImages.Jpeg());
        file.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        form.Add(file, "files", "a.jpg");
        (await Admin.PostAsync($"/api/admin/cars/{id}/photos", form)).EnsureSuccessStatusCode();

        var html = await Anonymous.GetStringAsync($"/api/share/cars/{id}");

        Assert.Matches($"""<meta property="og:image" content="http://localhost/uploads/cars/{id}/[0-9a-f]+\.jpg">""", html);
        Assert.Contains("summary_large_image", html);
    }

    [Fact]
    public async Task Text_from_the_database_is_html_encoded()
    {
        var car = NewCar(await CreateUniqueModelAsync()) with { DescriptionSq = "Super <script>alert('x')</script> \"veturë\"" };
        var id = await CreateCarAsync(car);

        var html = await Anonymous.GetStringAsync($"/api/share/cars/{id}");

        Assert.DoesNotContain("<script>", html);
        Assert.Contains("&lt;script&gt;", html);
    }

    [Fact]
    public async Task Sold_car_is_labelled_as_sold()
    {
        var id = await CreateCarAsync(NewCar(await CreateUniqueModelAsync()));
        await Admin.PatchAsync($"/api/admin/cars/{id}/status", System.Net.Http.Json.JsonContent.Create(new { status = "Sold" }));

        var html = await Anonymous.GetStringAsync($"/api/share/cars/{id}");

        Assert.Contains("(E shitur)", html);
    }

    [Fact]
    public async Task Unknown_car_returns_404()
    {
        var response = await Anonymous.GetAsync("/api/share/cars/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
