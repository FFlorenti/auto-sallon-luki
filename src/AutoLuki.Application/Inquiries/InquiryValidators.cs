using AutoLuki.Domain;
using FluentValidation;

namespace AutoLuki.Application.Inquiries;

public class CreateInquiryRequestValidator : AbstractValidator<CreateInquiryRequest>
{
    public CreateInquiryRequestValidator(TimeProvider timeProvider)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30)
            .Matches(@"^\+?[0-9\s\-/()]{6,29}$").WithMessage("Please enter a valid phone number.");
        RuleFor(x => x.Email).EmailAddress().MaximumLength(150).When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.CaptchaToken).NotEmpty().MaximumLength(2048);

        RuleFor(x => x.CarId).NotNull().When(x => x.Type == InquiryType.TestDrive)
            .WithMessage("A test drive must be for a specific car.");
        RuleFor(x => x.PreferredDate)
            .Must(d => d >= today && d <= today.AddMonths(3))
            .When(x => x.PreferredDate is not null)
            .WithMessage("The preferred date must be within the next 3 months.");
    }
}

public class InquiryQueryValidator : AbstractValidator<InquiryQuery>
{
    public InquiryQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status is not null);
    }
}
