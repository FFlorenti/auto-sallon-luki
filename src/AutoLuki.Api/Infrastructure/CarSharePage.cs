using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using AutoLuki.Application.Cars;
using AutoLuki.Domain;

namespace AutoLuki.Api.Infrastructure;

public static class CarSharePage
{
    private const int MaxDescriptionLength = 200;

    private static readonly HtmlEncoder Encoder = HtmlEncoder.Create(UnicodeRanges.All);

    public static string Title(CarDetailDto car)
    {
        var title = $"{car.Brand} {car.Model} {car.Year} · {FormatNumber(car.Price)} €";
        return car.Status == CarStatus.Sold ? $"{title} (E shitur)" : title;
    }

    public static string Description(CarDetailDto car)
    {
        var facts = $"{car.Year} · {FormatNumber(car.MileageKm)} km · {Fuel(car.FuelType)} · {Gearbox(car.Transmission)}";
        var text = (car.DescriptionSq ?? car.DescriptionEn ?? "").ReplaceLineEndings(" ").Trim();
        if (text.Length == 0)
        {
            return facts;
        }

        var room = MaxDescriptionLength - facts.Length - 2;
        return $"{facts}. {(text.Length > room ? text[..room].TrimEnd() + "…" : text)}";
    }

    public static string Render(CarDetailDto car, string pageUrl, string? imageUrl, string siteName)
    {
        var e = Encoder;
        var title = e.Encode(Title(car));
        var description = e.Encode(Description(car));
        var url = e.Encode(pageUrl);
        var site = e.Encode(siteName);
        var image = imageUrl is null ? null : e.Encode(imageUrl);

        var imageTags = image is null
            ? """<meta name="twitter:card" content="summary">"""
            : $"""
               <meta property="og:image" content="{image}">
                 <meta name="twitter:card" content="summary_large_image">
                 <meta name="twitter:image" content="{image}">
               """;

        return $"""
            <!doctype html>
            <html lang="sq">
            <head>
              <meta charset="utf-8">
              <title>{title} | {site}</title>
              <meta name="description" content="{description}">
              <link rel="canonical" href="{url}">
              <meta property="og:type" content="website">
              <meta property="og:site_name" content="{site}">
              <meta property="og:locale" content="sq_AL">
              <meta property="og:title" content="{title}">
              <meta property="og:description" content="{description}">
              <meta property="og:url" content="{url}">
              {imageTags}
            </head>
            <body>
              <h1>{title}</h1>
              <p>{description}</p>
              <p><a href="{url}">{site}</a></p>
            </body>
            </html>
            """;
    }

    private static string FormatNumber(decimal value) =>
        Math.Round(value).ToString("#,0", CultureInfo.InvariantCulture).Replace(',', ' ');

    private static string Fuel(FuelType fuel) => fuel switch
    {
        FuelType.Petrol => "Benzinë",
        FuelType.Diesel => "Naftë",
        FuelType.Hybrid => "Hibrid",
        FuelType.Electric => "Elektrike",
        FuelType.Lpg => "Benzinë/Gaz",
        _ => fuel.ToString(),
    };

    private static string Gearbox(Transmission transmission) =>
        transmission == Transmission.Automatic ? "Automatik" : "Manual";
}
