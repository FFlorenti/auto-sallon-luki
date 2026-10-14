using System.Net;
using AutoLuki.IntegrationTests.Infrastructure;

namespace AutoLuki.IntegrationTests;

public class WebsiteTests(PostgresFixture postgres)
    : IntegrationTestBase(postgres, new Dictionary<string, string?> { ["webroot"] = WebRoot })
{
    private const string IndexMarker = "<div id=\"root\"></div>";

    private static readonly string WebRoot = CreateWebRoot();

    private static string CreateWebRoot()
    {
        var folder = Path.Combine(Path.GetTempPath(), "autoluki-tests", "website-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(folder, "assets"));
        File.WriteAllText(Path.Combine(folder, "index.html"), $"<!doctype html><html><body>{IndexMarker}</body></html>");
        File.WriteAllText(Path.Combine(folder, "assets", "app-abc123.js"), "console.log('app')");
        return folder;
    }

    private HttpClient ClientWithUserAgent(string userAgent)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        return client;
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/favorites")]
    [InlineData("/cars/123")]
    [InlineData("/admin/cars")]
    public async Task Website_routes_return_the_app_page(string path)
    {
        var response = await Anonymous.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(IndexMarker, await response.Content.ReadAsStringAsync());
        Assert.Equal("no-cache", response.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task Responses_carry_security_headers()
    {
        var response = await Anonymous.GetAsync("/");

        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
    }

    [Fact]
    public async Task Hashed_assets_are_cached_for_a_long_time()
    {
        var response = await Anonymous.GetAsync("/assets/app-abc123.js");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("immutable", response.Headers.CacheControl?.ToString());
    }

    [Theory]
    [InlineData("/api/does-not-exist")]
    [InlineData("/uploads/cars/1/missing.jpg")]
    [InlineData("/assets/missing.js")]
    public async Task Unknown_api_and_file_paths_still_return_404(string path)
    {
        var response = await Anonymous.GetAsync(path);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Health_and_api_keep_working_next_to_the_website()
    {
        Assert.Equal("Healthy", await Anonymous.GetStringAsync("/health"));
        Assert.Equal(HttpStatusCode.OK, (await Anonymous.GetAsync("/api/brands")).StatusCode);
    }

    [Theory]
    [InlineData("WhatsApp/2.23.20.0 A")]
    [InlineData("facebookexternalhit/1.1 (+http://www.facebook.com/externalhit_uatext.php)")]
    public async Task Link_preview_bots_get_the_open_graph_page_for_a_car(string userAgent)
    {
        var id = await CreateCarAsync(NewCar(await CreateUniqueModelAsync(), price: 18_500m));

        var html = await ClientWithUserAgent(userAgent).GetStringAsync($"/cars/{id}");

        Assert.Contains("og:title", html);
        Assert.Contains("18 500 €", html);
        Assert.DoesNotContain(IndexMarker, html);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/130.0 Safari/537.36")]
    [InlineData("Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)")]
    public async Task People_and_search_engines_get_the_app_page_for_a_car(string userAgent)
    {
        var id = await CreateCarAsync(NewCar(await CreateUniqueModelAsync()));

        var html = await ClientWithUserAgent(userAgent).GetStringAsync($"/cars/{id}");

        Assert.Contains(IndexMarker, html);
    }
}
