using System.Collections.Generic;
using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.Persistence;

namespace PricingCalc.Model.UserSettings
{
    internal class UserSettingsModel : BaseModel, IUserSettingsModel
    {
        public UserSettingsModel(
            IReadOnlyCollection<IUserSettingsModelShard> shards,
            IStorage storage)
            : base(new View(shards))
        {
            History = new UserSettingsHistory(View, storage);
        }

        public IUserSettingsHistory History { get; }
    }
}
