using AutoLuki.Application.Common;
using AutoLuki.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoLuki.Application.Inquiries;

public interface IInquiryService
{
    Task<int> CreateAsync(CreateInquiryRequest request, string? remoteIp, CancellationToken cancellationToken = default);

    Task<PagedResult<InquiryDto>> SearchAsync(InquiryQuery query, CancellationToken cancellationToken = default);

    Task<InquiryDto> GetAsync(int id, CancellationToken cancellationToken = default);

    Task ChangeStatusAsync(int id, ChangeInquiryStatusRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public class InquiryService(
    IAppDbContext db,
    ICaptchaVerifier captchaVerifier,
    IInquiryNotificationQueue notificationQueue,
    IValidator<CreateInquiryRequest> createValidator,
    IValidator<InquiryQuery> queryValidator,
    ILogger<InquiryService> logger) : IInquiryService
{
    public async Task<int> CreateAsync(CreateInquiryRequest request, string? remoteIp, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        if (!await captchaVerifier.VerifyAsync(request.CaptchaToken, remoteIp, cancellationToken))
        {
            throw new BusinessRuleException("Spam protection check failed. Please try again.");
        }

        if (request.CarId is not null && !await db.Cars.AnyAsync(c => c.Id == request.CarId, cancellationToken))
        {
            throw new BusinessRuleException("The selected car does not exist.");
        }

        var inquiry = new Inquiry
        {
            CarId = request.CarId,
            Type = request.Type,
            Name = request.Name.Trim(),
            Phone = request.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            Message = request.Message.Trim(),
            PreferredDate = request.Type == InquiryType.TestDrive ? request.PreferredDate : null,
        };

        db.Inquiries.Add(inquiry);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Inquiry {InquiryId} created ({Type})", inquiry.Id, inquiry.Type);

        await notificationQueue.EnqueueAsync(inquiry.Id, cancellationToken);
        return inquiry.Id;
    }

    public async Task<PagedResult<InquiryDto>> SearchAsync(InquiryQuery query, CancellationToken cancellationToken = default)
    {
        await queryValidator.ValidateAndThrowAsync(query, cancellationToken);

        var inquiries = db.Inquiries.AsNoTracking();
        if (query.Status is not null)
        {
            inquiries = inquiries.Where(i => i.Status == query.Status);
        }

        var totalCount = await inquiries.CountAsync(cancellationToken);
        var items = await Project(inquiries
                .OrderByDescending(i => i.CreatedAt)
                .ThenByDescending(i => i.Id)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize))
            .ToListAsync(cancellationToken);

        return new PagedResult<InquiryDto>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<InquiryDto> GetAsync(int id, CancellationToken cancellationToken = default) =>
        await Project(db.Inquiries.AsNoTracking().Where(i => i.Id == id)).FirstOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException(nameof(Inquiry), id);

    public async Task ChangeStatusAsync(int id, ChangeInquiryStatusRequest request, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(request.Status))
        {
            throw new BusinessRuleException($"Unknown status '{request.Status}'.");
        }

        var inquiry = await db.Inquiries.FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Inquiry), id);

        inquiry.Status = request.Status;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var inquiry = await db.Inquiries.FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Inquiry), id);

        db.Inquiries.Remove(inquiry);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<InquiryDto> Project(IQueryable<Inquiry> inquiries) =>
        inquiries.Select(i => new InquiryDto(
            i.Id,
            i.Type,
            i.Name,
            i.Phone,
            i.Email,
            i.Message,
            i.PreferredDate,
            i.Status,
            i.CreatedAt,
            i.Car == null
                ? null
                : new InquiryCarDto(i.Car.Id, i.Car.Model.Brand.Name + " " + i.Car.Model.Name + " " + i.Car.Year)));
}
