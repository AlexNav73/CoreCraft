using CoreCraft.ChangesTracking;
using CoreCraft.Core;

namespace CoreCraft.Tests.Features;

public class ModelShardTests
{
    [Test]
    public void AddOrGetFrameWithMissingFrameTest()
    {
        IReadOnlyState<IMutableModelShard> modelShard = new FakeModelShard();
        var modelChanges = new ModelChanges(0);

        var registered = modelChanges.AddOrGet(modelShard.Create());

        Assert.That(registered, Is.Not.Null);
        Assert.That(registered, Is.TypeOf<FakeChangesFrame>());
    }

    [Test]
    public void AddOrGetFrameWithFrameTest()
    {
        IReadOnlyState<IMutableModelShard> modelShard = new FakeModelShard();
        var modelChanges = new ModelChanges(0);
        var frame = modelChanges.AddOrGet(new FakeChangesFrame());

        var registered = modelChanges.AddOrGet(modelShard.Create());

        Assert.That(registered, Is.Not.Null);
        Assert.That(ReferenceEquals(frame, registered), Is.True);
    }
}
