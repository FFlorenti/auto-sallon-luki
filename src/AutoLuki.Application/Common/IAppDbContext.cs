using AutoLuki.Domain;
using Microsoft.EntityFrameworkCore;

namespace AutoLuki.Application.Common;

public interface IAppDbContext
{
    DbSet<Brand> Brands { get; }
    DbSet<Model> Models { get; }
    DbSet<Car> Cars { get; }
    DbSet<CarPhoto> CarPhotos { get; }
    DbSet<Inquiry> Inquiries { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
