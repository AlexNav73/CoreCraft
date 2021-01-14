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
            : base(new View(shards))
        {
            _history = new UserSettingsHistory(this, storage);
        }

        public IUserSettingsHistory History => _history;

        internal override void RaiseEvent(MutateResult result)
        {
            if (result.Changes.HasChanges())
            {
                RaiseModelChangesEvent(result);
                _history.OnModelChanged(result);
            }
        }
    }
}
