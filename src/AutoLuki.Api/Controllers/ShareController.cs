using AutoLuki.Api.Infrastructure;
using AutoLuki.Application.Cars;
using Microsoft.AspNetCore.Mvc;

namespace AutoLuki.Api.Controllers;

[ApiController]
[Route("api/share")]
public class ShareController(ICarService carService, IConfiguration configuration) : ControllerBase
{
    [HttpGet("cars/{id:int}")]
    [Produces("text/html")]
    [ResponseCache(Duration = 300)]
    public async Task<ContentResult> Car(int id, CancellationToken cancellationToken)
    {
        var car = await carService.GetPublicAsync(id, cancellationToken);

        var origin = $"{Request.Scheme}://{Request.Host}";
        var photo = car.Photos.FirstOrDefault()?.Url;
        var imageUrl = photo is null ? null : photo.StartsWith('/') ? origin + photo : photo;
        var siteName = configuration["Site:Name"] ?? "Auto Sallon Luki";

        return Content(CarSharePage.Render(car, $"{origin}/cars/{id}", imageUrl, siteName), "text/html; charset=utf-8");
    }
}
