using AutoLuki.Application.Common;
using AutoLuki.Domain;
using AutoLuki.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutoLuki.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AdminUser>(options), IAppDbContext
{
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Model> Models => Set<Model>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<CarPhoto> CarPhotos => Set<CarPhoto>();
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<FuelType>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<Transmission>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<BodyType>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<CarStatus>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<InquiryType>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<InquiryStatus>().HaveConversion<string>().HaveMaxLength(20);
    }
}
