using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AutoLuki.Application.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoLuki.Infrastructure.Captcha;

public class TurnstileOptions
{
    public const string SectionName = "Turnstile";

    public string SecretKey { get; init; } = "";
}

public class TurnstileVerifier(
    HttpClient httpClient,
    IOptions<TurnstileOptions> options,
    ILogger<TurnstileVerifier> logger) : ICaptchaVerifier
{
    private const string VerifyUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

    public async Task<bool> VerifyAsync(string token, string? remoteIp, CancellationToken cancellationToken = default)
    {
        var form = new Dictionary<string, string>
        {
            ["secret"] = options.Value.SecretKey,
            ["response"] = token,
        };
        if (remoteIp is not null)
        {
            form["remoteip"] = remoteIp;
        }

        try
        {
            using var response = await httpClient.PostAsync(VerifyUrl, new FormUrlEncodedContent(form), cancellationToken);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<TurnstileResponse>(cancellationToken);

            if (result?.Success != true)
            {
                logger.LogWarning("Turnstile rejected a token: {Errors}", string.Join(",", result?.ErrorCodes ?? []));
            }

            return result?.Success == true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Turnstile verification request failed");
            return false;
        }
    }

    private sealed record TurnstileResponse(
        [property: JsonPropertyName("success")] bool Success,
        [property: JsonPropertyName("error-codes")] string[]? ErrorCodes);
}
