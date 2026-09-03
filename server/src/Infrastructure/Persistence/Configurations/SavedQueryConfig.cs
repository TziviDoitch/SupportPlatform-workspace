using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Persistence.Configurations;

public class SavedQueryConfig : IEntityTypeConfiguration<SavedQuery>
{
    public void Configure(EntityTypeBuilder<SavedQuery> builder)
    {
        builder.ToTable("saved_queries");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Name).HasMaxLength(200);
        builder.Property(q => q.DefinitionHash).HasMaxLength(80);
        builder.Property(q => q.OwnerUsername).HasMaxLength(64);
        builder.Property(q => q.TenantId).HasMaxLength(64);

        // Scoping is owner + tenant; list/find always filter on both.
        builder.HasIndex(q => new { q.TenantId, q.OwnerUsername });
    }
}
