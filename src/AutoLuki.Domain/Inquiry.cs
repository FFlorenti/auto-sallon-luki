namespace AutoLuki.Domain;

public class Inquiry : IHasCreatedAt
{
    public int Id { get; set; }

    public int? CarId { get; set; }

    public InquiryType Type { get; set; }
    public required string Name { get; set; }
    public required string Phone { get; set; }
    public string? Email { get; set; }
    public required string Message { get; set; }
    public DateOnly? PreferredDate { get; set; }
    public InquiryStatus Status { get; set; } = InquiryStatus.New;
    public DateTimeOffset CreatedAt { get; set; }

    public Car? Car { get; set; }
}
