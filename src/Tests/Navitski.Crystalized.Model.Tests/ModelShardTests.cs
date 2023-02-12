using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Lazy;

namespace Navitski.Crystalized.Model.Tests;

public class ModelShardTests
{
    [Test]
    public void AsMutableTest()
    {
        var modelShard = new FakeModelShard();
        var modelChanges = A.Fake<IWritableModelChanges>();

        A.CallTo(() => modelChanges.Register(A<FakeChangesFrame>.Ignored))
            .Returns(new FakeChangesFrame());

        var mutable = modelShard.AsMutable(modelChanges);

        A.CallTo(() => modelChanges.Register(A<FakeChangesFrame>.Ignored))
            .MustHaveHappenedOnceExactly();

        Assert.That(mutable, Is.Not.Null);
        Assert.That(mutable, Is.TypeOf<MutableFakeModelShard>());
    }

    [Test]
    public void AsMutableCheckRegisteredChangesFrameTest()
    {
        var modelShard = new FakeModelShard();
        var modelChanges = A.Fake<IWritableModelChanges>();
        IChangesFrame? registeredFrame = null;

        A.CallTo(() => modelChanges.Register(A<FakeChangesFrame>.Ignored))
            .Invokes(c => registeredFrame = c.Arguments.Single() as IChangesFrame)
            .Returns(new FakeChangesFrame());

        var mutable = modelShard.AsMutable(modelChanges);

        Assert.That(registeredFrame, Is.Not.Null);
        Assert.That(registeredFrame, Is.TypeOf<FakeChangesFrame>());
    }

    [Test]
    public void AsMutableWithCopyFeatureCreatesCoWCollectionsAndRelationsTest()
    {
        var modelShard = new FakeModelShard();

        var mutable = modelShard.AsMutable(null);

        Assert.That(mutable.FirstCollection, Is.TypeOf<CoWCollection<FirstEntity, FirstEntityProperties>>());
        Assert.That(mutable.OneToOneRelation, Is.TypeOf<CoWRelation<FirstEntity, SecondEntity>>());
    }

    [Test]
    public void AsMutableWithTrackFeatureCreatesTrackableCollectionsAndRelationsTest()
    {
        var modelShard = new FakeModelShard();
        var modelChanges = A.Fake<IWritableModelChanges>();

        var mutable = modelShard.AsMutable(modelChanges);

        Assert.That(mutable.FirstCollection, Is.TypeOf<TrackableCollection<FirstEntity, FirstEntityProperties>>());
        Assert.That(mutable.OneToOneRelation, Is.TypeOf<TrackableRelation<FirstEntity, SecondEntity>>());
    }

    [Test]
    public void AsReadOnlyWithCopyFeatureCreatesRegularCollectionsAndRelationsTest()
    {
        var modelShard = new FakeModelShard();

        var mutable = modelShard.AsMutable(null);
        var readOnly = ((ICanBeReadOnly<IFakeModelShard>)mutable).AsReadOnly();

        Assert.That(readOnly.FirstCollection, Is.TypeOf<Collection<FirstEntity, FirstEntityProperties>>());
        Assert.That(readOnly.OneToOneRelation, Is.TypeOf<Relation<FirstEntity, SecondEntity>>());
    }

    [Test]
    public void AsReadOnlyWithTrackFeatureCreatesRegularCollectionsAndRelationsTest()
    {
        var modelShard = new FakeModelShard();
        var modelChanges = A.Fake<IWritableModelChanges>();

        var mutable = modelShard.AsMutable(modelChanges);
        var readOnly = ((ICanBeReadOnly<IFakeModelShard>)mutable).AsReadOnly();

        Assert.That(readOnly.FirstCollection, Is.TypeOf<Collection<FirstEntity, FirstEntityProperties>>());
        Assert.That(readOnly.OneToOneRelation, Is.TypeOf<Relation<FirstEntity, SecondEntity>>());
    }
}
