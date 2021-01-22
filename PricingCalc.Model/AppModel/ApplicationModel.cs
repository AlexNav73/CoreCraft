using System.Collections.Generic;
using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.Persistence;

namespace PricingCalc.Model.AppModel
{
    internal class ApplicationModel : BaseModel, IApplicationModel
    {
        private readonly ApplicationHistory _history;

        public ApplicationModel(
            IReadOnlyCollection<IApplicationModelShard> shards,
            IStorage storage,
            IJobService jobService)
            : base(new View(shards), storage, jobService)
        {
            _history = new ApplicationHistory(this);
        }

        public IApplicationHistory History => _history;

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
