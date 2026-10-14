using AutoLuki.Application.Common;
using AutoLuki.Domain;
using Microsoft.EntityFrameworkCore;

namespace AutoLuki.Application.Dashboard;

public record DashboardDto(
    int CarsInStock,
    int CarsSoldTotal,
    int CarsSoldThisMonth,
    int InquiriesTotal,
    int InquiriesNew,
    int InquiriesThisMonth);

public interface IDashboardService
{
    Task<DashboardDto> GetAsync(CancellationToken cancellationToken = default);
}

public class DashboardService(IAppDbContext db, TimeProvider timeProvider) : IDashboardService
{
    public async Task<DashboardDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

        var carCounts = await db.Cars
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

        var soldThisMonth = await db.Cars.CountAsync(c => c.SoldAt >= monthStart, cancellationToken);
        var inquiriesTotal = await db.Inquiries.CountAsync(cancellationToken);
        var inquiriesNew = await db.Inquiries.CountAsync(i => i.Status == InquiryStatus.New, cancellationToken);
        var inquiriesThisMonth = await db.Inquiries.CountAsync(i => i.CreatedAt >= monthStart, cancellationToken);

        return new DashboardDto(
            carCounts.GetValueOrDefault(CarStatus.Available),
            carCounts.GetValueOrDefault(CarStatus.Sold),
            soldThisMonth,
            inquiriesTotal,
            inquiriesNew,
            inquiriesThisMonth);
    }
}
