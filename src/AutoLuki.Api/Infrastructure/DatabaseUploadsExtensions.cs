using AutoLuki.Infrastructure.Storage;

namespace AutoLuki.Api.Infrastructure;

public static class DatabaseUploadsExtensions
{
    public static WebApplication MapDatabaseUploads(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            if (scope.ServiceProvider.GetService<DatabaseFileStorage>() is null)
            {
                return app;
            }
        }

        app.MapGet(DatabaseFileStorage.RequestPath + "/{**name}",
            async (string name, DatabaseFileStorage storage, HttpContext context, CancellationToken cancellationToken) =>
            {
                var file = await storage.FindAsync(name, cancellationToken);
                if (file is null)
                {
                    return Results.NotFound();
                }

                context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
                return Results.File(file.Content, file.ContentType);
            })
            .ExcludeFromDescription();

        return app;
    }
}
