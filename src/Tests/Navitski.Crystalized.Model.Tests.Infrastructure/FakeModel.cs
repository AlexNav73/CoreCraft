using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.Core;

namespace Navitski.Crystalized.Model.Tests.Infrastructure;

public class FakeModel : DomainModel
{
    public FakeModel(IEnumerable<IModelShard> shards)
        : base(shards, new ModelConfiguration() { Scheduler = new SyncScheduler() })
    {
        History = new FakeModelHistory(this);
    }

    public FakeModelHistory History { get; set; }
}
