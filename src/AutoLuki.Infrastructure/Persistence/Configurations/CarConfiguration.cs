using AutoLuki.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoLuki.Infrastructure.Persistence.Configurations;

public class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> builder)
    {
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Cars_Price", "\"Price\" > 0");
            t.HasCheckConstraint("CK_Cars_MileageKm", "\"MileageKm\" >= 0");
            t.HasCheckConstraint("CK_Cars_Year", "\"Year\" >= 1950");
        });

        builder.Property(c => c.Price).HasPrecision(10, 2);
        builder.Property(c => c.Color).HasMaxLength(30);
        builder.Property(c => c.Vin).HasMaxLength(17);
        builder.Property(c => c.DescriptionSq).HasMaxLength(4000);
        builder.Property(c => c.DescriptionEn).HasMaxLength(4000);

        builder.HasOne(c => c.Model)
            .WithMany()
            .HasForeignKey(c => c.ModelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.HasIndex(c => new { c.Status, c.CreatedAt });
        builder.HasIndex(c => c.Price);
        builder.HasIndex(c => c.Year);
        builder.HasIndex(c => c.MileageKm);
    }
}

public class CarPhotoConfiguration : IEntityTypeConfiguration<CarPhoto>
{
    public void Configure(EntityTypeBuilder<CarPhoto> builder)
    {
        builder.Property(p => p.BlobName).HasMaxLength(200);
        builder.Property(p => p.ThumbBlobName).HasMaxLength(200);

        builder.HasOne(p => p.Car)
            .WithMany(c => c.Photos)
            .HasForeignKey(p => p.CarId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(p => !p.Car.IsDeleted);

        builder.HasIndex(p => new { p.CarId, p.SortOrder });
    }
}
