using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;

namespace Navitski.Crystalized.Model.Tests;

public class ModelShardTests
{
    [Test]
    public void AsMutableApplyAllFeaturesTest()
    {
        var modelShard = new FakeModelShard();
        var feature1 = A.Fake<IFeature>();
        var feature2 = A.Fake<IFeature>();

        var mutable = modelShard.AsMutable(new[] { feature1, feature2 });

        A.CallTo(() => feature1.Decorate(A<IFeatureContext>.Ignored, A<IMutableCollection<FirstEntity, FirstEntityProperties>>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => feature2.Decorate(A<IFeatureContext>.Ignored, A<IMutableCollection<FirstEntity, FirstEntityProperties>>.Ignored))
            .MustHaveHappenedOnceExactly();

        Assert.That(mutable, Is.Not.Null);
        Assert.That(mutable, Is.TypeOf<MutableFakeModelShard>());
    }

    [Test]
    public void GetOrAddFrameWithMissingFrameTest()
    {
        var modelShard = new FakeModelShard();
        var modelChanges = new ModelChanges();

        var registered = modelShard.GetOrAddFrame(modelChanges);

        Assert.That(registered, Is.Not.Null);
        Assert.That(registered, Is.TypeOf<FakeChangesFrame>());
    }

    [Test]
    public void GetOrAddFrameWithFrameTest()
    {
        var modelShard = new FakeModelShard();
        var modelChanges = new ModelChanges();
        var frame = modelChanges.Register(() => new FakeChangesFrame());

        var registered = modelShard.GetOrAddFrame(modelChanges);

        Assert.That(registered, Is.Not.Null);
        Assert.That(ReferenceEquals(frame, registered), Is.True);
    }
}
