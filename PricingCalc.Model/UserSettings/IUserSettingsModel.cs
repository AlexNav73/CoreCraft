using PricingCalc.Model.Engine;

namespace PricingCalc.Model.UserSettings
{
    public interface IUserSettingsModel : IBaseModel
    {
        IUserSettingsHistory History { get; }
    }
}
