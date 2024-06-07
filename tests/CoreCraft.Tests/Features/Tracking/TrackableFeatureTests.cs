using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Features.Tracking;

namespace CoreCraft.Tests.Features.Tracking;

public class TrackableFeatureTests
{
    [Test]
    public void DecorateTest()
    {
        var modelChanges = A.Fake<IMutableModelChanges>();
        var feature = new TrackableFeature(modelChanges);
        var frameFactory = A.Fake<IFrameFactory>();
        var frame = A.Fake<IChangesFrameEx>();

        A.CallTo(() => modelChanges.AddOrGet(A<IChangesFrame>.Ignored))
            .Returns(frame);

        var collection = feature.Decorate(frameFactory, A.Fake<IMutableCollection<FirstEntity, FirstEntityProperties>>());
        var relation = feature.Decorate(frameFactory, A.Fake<IMutableRelation<FirstEntity, SecondEntity>>());

        A.CallTo(() => frame.Get(A<ICollection<FirstEntity, FirstEntityProperties>>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => frame.Get(A<IRelation<FirstEntity, SecondEntity>>.Ignored))
            .MustHaveHappenedOnceExactly();

        Assert.That(collection, Is.Not.Null);
        Assert.That(collection, Is.TypeOf<TrackableCollection<FirstEntity, FirstEntityProperties>>());
        Assert.That(relation, Is.Not.Null);
        Assert.That(relation, Is.TypeOf<TrackableRelation<FirstEntity, SecondEntity>>());
    }

    [Test]
    public void DecorateWithMissingCollectionOrRelationTest()
    {
        var modelChanges = A.Fake<IMutableModelChanges>();
        var feature = new TrackableFeature(modelChanges);
        var frameFactory = A.Fake<IFrameFactory>();
        var frame = A.Fake<IChangesFrameEx>();

        A.CallTo(() => modelChanges.AddOrGet(A<IChangesFrame>.Ignored))
            .Returns(frame);

        A.CallTo(() => frame.Get(A<ICollection<FirstEntity, FirstEntityProperties>>.Ignored))
            .Returns(null);
        A.CallTo(() => frame.Get(A<IRelation<FirstEntity, SecondEntity>>.Ignored))
            .Returns(null);

        var originalCollection = A.Fake<IMutableCollection<FirstEntity, FirstEntityProperties>>();
        var collection = feature.Decorate(frameFactory, originalCollection);
        var originalRelation = A.Fake<IMutableRelation<FirstEntity, SecondEntity>>();
        var relation = feature.Decorate(frameFactory, originalRelation);

        Assert.That(collection, Is.Not.Null);
        Assert.That(ReferenceEquals(collection, originalCollection), Is.True);
        Assert.That(relation, Is.Not.Null);
        Assert.That(ReferenceEquals(relation, originalRelation), Is.True);
    }

    [Test]
    public void DoNotTreatDifferentModelShardsAsHavingTheSameTypeTest()
    {
        var modelChanges = new ModelChanges(0);
        var feature = new TrackableFeature(modelChanges);
        var frameFactory = new FakeModelShard();

        var otherModelShard = A.Fake<IFrameFactory>();
        var otherFrame = A.Fake<IChangesFrame>(c => c.Implements<IChangesFrameEx>());

        A.CallTo(() => otherModelShard.Create()).Returns(otherFrame);

        modelChanges.AddOrGet(otherFrame);

        _ = feature.Decorate(otherModelShard, A.Fake<IMutableCollection<FirstEntity, FirstEntityProperties>>());
        _ = feature.Decorate(frameFactory, (IMutableCollection<FirstEntity, FirstEntityProperties>)frameFactory.FirstCollection);

        Assert.DoesNotThrow(() => feature.Decorate(frameFactory, (IMutableCollection<SecondEntity, SecondEntityProperties>)frameFactory.SecondCollection));
    }
}
