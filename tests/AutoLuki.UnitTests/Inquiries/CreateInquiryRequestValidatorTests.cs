using AutoLuki.Application.Inquiries;
using AutoLuki.Domain;
using Microsoft.Extensions.Time.Testing;

namespace AutoLuki.UnitTests.Inquiries;

public class CreateInquiryRequestValidatorTests
{
    private static readonly DateOnly Today = new(2026, 10, 7);

    private readonly CreateInquiryRequestValidator _validator =
        new(new FakeTimeProvider(new DateTimeOffset(2026, 10, 7, 9, 0, 0, TimeSpan.Zero)));

    private static CreateInquiryRequest ValidRequest() => new(
        CarId: 3, Type: InquiryType.General, Name: "Arben", Phone: "+383 44 123 456",
        Email: null, Message: "Is it still available?", PreferredDate: null, CaptchaToken: "token");

    [Fact]
    public void Valid_request_passes()
    {
        Assert.True(_validator.Validate(ValidRequest()).IsValid);
    }

    [Theory]
    [InlineData("044123456")]
    [InlineData("+383 44 123 456")]
    [InlineData("+49 (0) 171 1234567")]
    [InlineData("044/123-456")]
    public void Common_phone_formats_are_accepted(string phone)
    {
        Assert.True(_validator.Validate(ValidRequest() with { Phone = phone }).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("call me")]
    [InlineData("044<script>")]
    public void Invalid_phone_fails(string phone)
    {
        var result = _validator.Validate(ValidRequest() with { Phone = phone });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateInquiryRequest.Phone));
    }

    [Fact]
    public void Email_is_optional_but_must_be_valid_when_given()
    {
        Assert.True(_validator.Validate(ValidRequest() with { Email = "" }).IsValid);
        Assert.False(_validator.Validate(ValidRequest() with { Email = "not-an-email" }).IsValid);
    }

    [Fact]
    public void Test_drive_requires_a_car()
    {
        var result = _validator.Validate(ValidRequest() with { Type = InquiryType.TestDrive, CarId = null });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateInquiryRequest.CarId));
    }

    [Fact]
    public void General_inquiry_without_car_is_fine()
    {
        Assert.True(_validator.Validate(ValidRequest() with { CarId = null }).IsValid);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100)]
    public void Preferred_date_outside_window_fails(int daysFromToday)
    {
        var request = ValidRequest() with { Type = InquiryType.TestDrive, PreferredDate = Today.AddDays(daysFromToday) };

        Assert.Contains(_validator.Validate(request).Errors, e => e.PropertyName == nameof(CreateInquiryRequest.PreferredDate));
    }

    [Fact]
    public void Preferred_date_today_is_fine()
    {
        var request = ValidRequest() with { Type = InquiryType.TestDrive, PreferredDate = Today };

        Assert.True(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Missing_captcha_token_fails()
    {
        Assert.False(_validator.Validate(ValidRequest() with { CaptchaToken = "" }).IsValid);
    }
}
