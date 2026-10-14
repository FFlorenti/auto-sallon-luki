using System.Text.RegularExpressions;

namespace AutoLuki.Api.Infrastructure;

public static partial class WebsiteExtensions
{
    private const string FallbackRoute = "{*path:regex(^(?!api/|uploads/|assets/|swagger|openapi|health).*$)}";

    [GeneratedRegex(@"^/cars/(\d+)/?$")]
    private static partial Regex CarPage();

    [GeneratedRegex(
        "facebookexternalhit|Facebot|WhatsApp|Viber|TelegramBot|Twitterbot|LinkedInBot|Slackbot|Discordbot|SkypeUriPreview|Pinterest|Snapchat",
        RegexOptions.IgnoreCase)]
    private static partial Regex PreviewBot();

    public static bool HasWebsite(this WebApplication app) =>
        app.Environment.WebRootFileProvider.GetFileInfo("index.html").Exists;

    public static WebApplication UseWebsite(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;
            headers.XContentTypeOptions = "nosniff";
            headers.XFrameOptions = "DENY";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

            var car = CarPage().Match(context.Request.Path.Value ?? "");
            if (car.Success && PreviewBot().IsMatch(context.Request.Headers.UserAgent.ToString()))
            {
                context.Request.Path = $"/api/share/cars/{car.Groups[1].Value}";
            }

            await next();
        });

        app.UseDefaultFiles();
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = context =>
            {
                var isHashedAsset = context.Context.Request.Path.StartsWithSegments("/assets");
                context.Context.Response.Headers.CacheControl =
                    isHashedAsset ? "public, max-age=31536000, immutable" : "no-cache";
            },
        });

        return app;
    }

    public static WebApplication MapWebsiteFallback(this WebApplication app)
    {
        app.MapFallbackToFile(FallbackRoute, "index.html", new StaticFileOptions
        {
            OnPrepareResponse = context => context.Context.Response.Headers.CacheControl = "no-cache",
        });

        return app;
    }
}
