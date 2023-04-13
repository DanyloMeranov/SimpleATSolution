using BasePage;

namespace SetupTypes.Position_Type
{
    public class PositionTypePage : BasePage<BasePageElementMap, BasePageValidator<BasePageElementMap>>
    {
        public PositionTypePage()
            : base("commonm/common/positiontype/index?menuId=common_positiontype_index")//don't forget to set entity relative local path
        {
        }
    }
}
