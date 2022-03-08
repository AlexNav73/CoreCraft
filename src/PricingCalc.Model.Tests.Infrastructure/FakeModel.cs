using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Tests.Infrastructure;

public class FakeModel : BaseModel
{
    public FakeModel(IEnumerable<IModelShard> shards)
        : base(shards, new ModelConfiguration() { Scheduler = new SyncScheduler() })
    {
        History = new FakeModelHistory(this);
    }

    public FakeModelHistory History { get; set; }
}
