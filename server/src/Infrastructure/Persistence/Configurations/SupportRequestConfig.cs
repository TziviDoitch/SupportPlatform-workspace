using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Persistence.Configurations;

public class SupportRequestConfig : IEntityTypeConfiguration<SupportRequest>
{
    public void Configure(EntityTypeBuilder<SupportRequest> builder)
    {
        builder.ToTable("support_requests");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.TenantId).HasMaxLength(64);
        builder.Property(r => r.SupportDomainCode).HasMaxLength(64);
        builder.Property(r => r.StatusCode).HasMaxLength(64);
        builder.Property(r => r.AmountRequested).HasPrecision(18, 2);
        builder.Property(r => r.AmountApproved).HasPrecision(18, 2);

        builder.HasIndex(r => r.TenantId);
        builder.HasIndex(r => new { r.TenantId, r.SupportYear });

        builder.HasOne(r => r.SubmittingBody).WithMany().HasForeignKey(r => r.SubmittingBodyId);
        builder.HasOne<ReferenceDomain>().WithMany().HasForeignKey(r => r.SupportDomainCode);
        builder.HasOne<ReferenceStatus>().WithMany().HasForeignKey(r => r.StatusCode);
    }
}
