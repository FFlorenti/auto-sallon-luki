using AutoLuki.Infrastructure.Storage;
using Microsoft.Extensions.FileProviders;

namespace AutoLuki.Api.Infrastructure;

public static class LocalUploadsExtensions
{
    public static WebApplication UseLocalUploads(this WebApplication app)
    {
        var storage = app.Services.GetService<LocalFileStorage>();
        if (storage is null)
        {
            return app;
        }

        Directory.CreateDirectory(storage.RootPath);
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(storage.RootPath),
            RequestPath = LocalFileStorage.RequestPath,
            OnPrepareResponse = ctx =>
                ctx.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable",
        });

        return app;
    }
}
