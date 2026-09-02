using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Persistence.Configurations;

public class ReferenceStatusConfig : ReferenceItemConfig<ReferenceStatus>
{
    protected override string TableName => "reference_statuses";
}
