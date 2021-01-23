using System.Collections.Generic;
using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.Persistence;

namespace PricingCalc.Model.AppModel
{
    internal class ApplicationModel : BaseModel, IApplicationModel
    {
        public ApplicationModel(
            IReadOnlyCollection<IApplicationModelShard> shards,
            IStorage storage,
            IJobService jobService)
            : base(new View(shards), storage, jobService)
        {
            History = new ApplicationHistory(this);
        }

        public IApplicationHistory History { get; }
    }
}
