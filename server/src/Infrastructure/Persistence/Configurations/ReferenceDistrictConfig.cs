using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Persistence.Configurations;

public class ReferenceDistrictConfig : ReferenceItemConfig<ReferenceDistrict>
{
    protected override string TableName => "reference_districts";
}
