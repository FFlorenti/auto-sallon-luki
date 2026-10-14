using AutoLuki.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoLuki.Infrastructure.Persistence.Configurations;

public class InquiryConfiguration : IEntityTypeConfiguration<Inquiry>
{
    public void Configure(EntityTypeBuilder<Inquiry> builder)
    {
        builder.Property(i => i.Name).HasMaxLength(100);
        builder.Property(i => i.Phone).HasMaxLength(30);
        builder.Property(i => i.Email).HasMaxLength(150);
        builder.Property(i => i.Message).HasMaxLength(2000);

        builder.HasOne(i => i.Car)
            .WithMany()
            .HasForeignKey(i => i.CarId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(i => new { i.Status, i.CreatedAt });
    }
}
