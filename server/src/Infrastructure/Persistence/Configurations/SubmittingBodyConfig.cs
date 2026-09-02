using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Persistence.Configurations;

public class SubmittingBodyConfig : IEntityTypeConfiguration<SubmittingBody>
{
    public void Configure(EntityTypeBuilder<SubmittingBody> builder)
    {
        builder.ToTable("submitting_bodies");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).HasMaxLength(200);
        builder.Property(b => b.TenantId).HasMaxLength(64);
        builder.Property(b => b.BodyTypeCode).HasMaxLength(64);
        builder.Property(b => b.DistrictCode).HasMaxLength(64);
        builder.HasIndex(b => b.TenantId);
        builder.HasOne<Tenant>().WithMany().HasForeignKey(b => b.TenantId);
        builder.HasOne<ReferenceBodyType>().WithMany().HasForeignKey(b => b.BodyTypeCode);
        builder.HasOne<ReferenceDistrict>().WithMany().HasForeignKey(b => b.DistrictCode);
    }
}
