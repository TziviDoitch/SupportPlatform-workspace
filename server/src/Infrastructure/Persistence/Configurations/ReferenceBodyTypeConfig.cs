using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Persistence.Configurations;

public class ReferenceBodyTypeConfig : ReferenceItemConfig<ReferenceBodyType>
{
    protected override string TableName => "reference_body_types";
}
