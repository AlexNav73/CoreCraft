using System.Collections.Generic;
using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Tests.Infrastructure
{
    public class FakeModel : BaseModel
    {
        public FakeModel(IEnumerable<IModelShard> shards, IJobService jobService)
            : base(shards, jobService)
        {
            History = new FakeModelHistory(this);
        }

        public FakeModelHistory History { get; set; }
    }
}
