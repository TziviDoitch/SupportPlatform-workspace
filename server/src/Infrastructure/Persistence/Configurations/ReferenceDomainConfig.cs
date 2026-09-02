using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Persistence.Configurations;

public class ReferenceDomainConfig : ReferenceItemConfig<ReferenceDomain>
{
    protected override string TableName => "reference_domains";
}
