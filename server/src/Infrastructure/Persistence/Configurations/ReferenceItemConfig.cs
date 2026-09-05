using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Persistence.Configurations;

public abstract class ReferenceItemConfig<T> : IEntityTypeConfiguration<T> where T : ReferenceItem
{
    protected abstract string TableName { get; }

    public void Configure(EntityTypeBuilder<T> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(r => r.Code);
        builder.Property(r => r.Code).HasMaxLength(64);
        builder.Property(r => r.Label).HasMaxLength(200);
    }
}
