using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Persistence.Configurations;

public class FilterFieldRegistryEntryConfig : IEntityTypeConfiguration<FilterFieldRegistryEntry>
{
    public void Configure(EntityTypeBuilder<FilterFieldRegistryEntry> builder)
    {
        builder.ToTable("filter_field_registry");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(64);
        builder.Property(e => e.Label).HasMaxLength(200);
        builder.Property(e => e.Kind).HasMaxLength(32);
        builder.Property(e => e.ReferenceList).HasMaxLength(64);

        var toCsv = new ValueConverter<IReadOnlyList<string>, string>(
            v => string.Join(',', v),
            v => v.Length == 0 ? Array.Empty<string>() : v.Split(',', StringSplitOptions.None));

        var compare = new ValueComparer<IReadOnlyList<string>>(
            (a, b) => a!.SequenceEqual(b!),
            v => v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
            v => v.ToList());

        builder.Property(e => e.Operators).HasConversion(toCsv).HasMaxLength(64).Metadata
            .SetValueComparer(compare);
    }
}
