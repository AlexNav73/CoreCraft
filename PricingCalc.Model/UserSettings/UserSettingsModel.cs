using System.Collections.Generic;
using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.Persistence;

namespace PricingCalc.Model.UserSettings
{
    internal class UserSettingsModel : BaseModel, IUserSettingsModel
    {
        private readonly UserSettingsHistory _history;

        public UserSettingsModel(
            IReadOnlyCollection<IUserSettingsModelShard> shards,
            IStorage storage)
            : base(new View(shards), storage)
        {
            _history = new UserSettingsHistory(this);
        }

        public IUserSettingsHistory History => _history;

        public override void RaiseEvent(ModelChangeResult result)
        {
            if (result.Changes.HasChanges())
            {
                base.RaiseEvent(result);
                _history.OnModelChanged(result);
            }
        }
    }
}
