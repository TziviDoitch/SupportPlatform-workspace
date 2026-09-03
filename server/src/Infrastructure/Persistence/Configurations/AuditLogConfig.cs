using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Persistence.Configurations;

public class AuditLogConfig : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_log");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.User).HasMaxLength(64);
        builder.Property(a => a.Action).HasMaxLength(32);
        builder.Property(a => a.EntityType).HasMaxLength(64);
        builder.Property(a => a.EntityId).HasMaxLength(64);
        builder.Property(a => a.CorrelationId).HasMaxLength(64);

        builder.HasIndex(a => a.OccurredAt);
    }
}
