using PricingCalc.Model.Engine;

namespace PricingCalc.Model.AppModel
{
    public interface IApplicationModel : IBaseModel
    {
        IApplicationHistory History { get; }
    }
}
