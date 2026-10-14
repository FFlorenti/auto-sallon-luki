using AutoLuki.Domain;

namespace AutoLuki.Application.Inquiries;

public record CreateInquiryRequest(
    int? CarId,
    InquiryType Type,
    string Name,
    string Phone,
    string? Email,
    string Message,
    DateOnly? PreferredDate,
    string CaptchaToken);

public record InquiryCarDto(int Id, string Title);

public record InquiryDto(
    int Id,
    InquiryType Type,
    string Name,
    string Phone,
    string? Email,
    string Message,
    DateOnly? PreferredDate,
    InquiryStatus Status,
    DateTimeOffset CreatedAt,
    InquiryCarDto? Car);

public record InquiryQuery
{
    public InquiryStatus? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public record ChangeInquiryStatusRequest(InquiryStatus Status);
