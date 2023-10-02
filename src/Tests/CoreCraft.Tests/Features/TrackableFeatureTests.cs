using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Features;

namespace CoreCraft.Tests.Features;

public class TrackableFeatureTests
{
    [Test]
    public void DecorateTest()
    {
        var feature = new TrackableFeature(A.Fake<IMutableModelChanges>());
        var featureContext = A.Fake<IFeatureContext>();
        var frame = A.Fake<IChangesFrame>();

        A.CallTo(() => featureContext.GetOrAddFrame(A<IMutableModelChanges>.Ignored))
            .Returns(frame);

        var collection = feature.Decorate(featureContext, A.Fake<IMutableCollection<FirstEntity, FirstEntityProperties>>());
        var relation = feature.Decorate(featureContext, A.Fake<IMutableRelation<FirstEntity, SecondEntity>>());

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
        var feature = new TrackableFeature(A.Fake<IMutableModelChanges>());
        var featureContext = A.Fake<IFeatureContext>();
        var frame = A.Fake<IChangesFrame>();

        A.CallTo(() => featureContext.GetOrAddFrame(A<IMutableModelChanges>.Ignored))
            .Returns(frame);

        A.CallTo(() => frame.Get(A<ICollection<FirstEntity, FirstEntityProperties>>.Ignored))
            .Returns(null);
        A.CallTo(() => frame.Get(A<IRelation<FirstEntity, SecondEntity>>.Ignored))
            .Returns(null);

        var originalCollection = A.Fake<IMutableCollection<FirstEntity, FirstEntityProperties>>();
        var collection = feature.Decorate(featureContext, originalCollection);
        var originalRelation = A.Fake<IMutableRelation<FirstEntity, SecondEntity>>();
        var relation = feature.Decorate(featureContext, originalRelation);

        Assert.That(collection, Is.Not.Null);
        Assert.That(ReferenceEquals(collection, originalCollection), Is.True);
        Assert.That(relation, Is.Not.Null);
        Assert.That(ReferenceEquals(relation, originalRelation), Is.True);
    }
}
