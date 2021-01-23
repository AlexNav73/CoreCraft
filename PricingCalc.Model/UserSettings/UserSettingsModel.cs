using System.Collections.Generic;
using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.Persistence;

namespace PricingCalc.Model.UserSettings
{
    internal class UserSettingsModel : BaseModel, IUserSettingsModel
    {
        public UserSettingsModel(
            IReadOnlyCollection<IUserSettingsModelShard> shards,
            IStorage storage,
            IJobService jobService)
            : base(new View(shards), storage, jobService)
        {
            History = new UserSettingsHistory(this);
        }

        public IUserSettingsHistory History { get; }
    }
}
