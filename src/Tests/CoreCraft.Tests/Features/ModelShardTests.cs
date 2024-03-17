using CoreCraft.ChangesTracking;
using CoreCraft.Core;

namespace CoreCraft.Tests.Features;

public class ModelShardTests
{
    [Test]
    public void AsMutableApplyAllFeaturesTest()
    {
        var modelShard = new FakeModelShard();
        var feature1 = A.Fake<IFeature>();
        var feature2 = A.Fake<IFeature>();

        var mutable = modelShard.AsMutable([feature1, feature2]);

        A.CallTo(() => feature1.Decorate(A<IFrameFactory>.Ignored, A<IMutableCollection<FirstEntity, FirstEntityProperties>>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => feature2.Decorate(A<IFrameFactory>.Ignored, A<IMutableCollection<FirstEntity, FirstEntityProperties>>.Ignored))
            .MustHaveHappenedOnceExactly();

        Assert.That(mutable, Is.Not.Null);
        Assert.That(mutable, Is.TypeOf<MutableFakeModelShard>());
    }

    [Test]
    public void AddOrGetFrameWithMissingFrameTest()
    {
        IFrameFactory modelShard = new FakeModelShard();
        var modelChanges = new ModelChanges();

        var registered = modelChanges.AddOrGet(modelShard.Create());

        Assert.That(registered, Is.Not.Null);
        Assert.That(registered, Is.TypeOf<FakeChangesFrame>());
    }

    [Test]
    public void AddOrGetFrameWithFrameTest()
    {
        IFrameFactory modelShard = new FakeModelShard();
        var modelChanges = new ModelChanges();
        var frame = modelChanges.AddOrGet(new FakeChangesFrame());

        var registered = modelChanges.AddOrGet(modelShard.Create());

        Assert.That(registered, Is.Not.Null);
        Assert.That(ReferenceEquals(frame, registered), Is.True);
    }
}
