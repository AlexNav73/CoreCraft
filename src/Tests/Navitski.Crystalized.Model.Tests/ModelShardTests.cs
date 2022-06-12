using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Tests.Infrastructure.Commands;

namespace Navitski.Crystalized.Model.Tests;

public class ModelShardTests
{
    [Test]
    public void AsTrackableTest()
    {
        var modelShard = new FakeModelShard();
        var modelChanges = A.Fake<IWritableModelChanges>();

        A.CallTo(() => modelChanges.Register(A<FakeChangesFrame>.Ignored))
            .Returns(new FakeChangesFrame());

        var trackable = modelShard.AsTrackable(modelChanges);

        A.CallTo(() => modelChanges.Register(A<FakeChangesFrame>.Ignored))
            .MustHaveHappenedOnceExactly();

        Assert.That(trackable, Is.Not.Null);
        Assert.That(trackable, Is.TypeOf<TrackableFakeModelShard>());
    }

    [Test]
    public void AsTrackableCheckRegisteredChangesFrameTest()
    {
        var modelShard = new FakeModelShard();
        var modelChanges = A.Fake<IWritableModelChanges>();
        IChangesFrame? registeredFrame = null;

        A.CallTo(() => modelChanges.Register(A<FakeChangesFrame>.Ignored))
            .Invokes(c => registeredFrame = c.Arguments.Single() as IChangesFrame)
            .Returns(new FakeChangesFrame());

        var trackable = modelShard.AsTrackable(modelChanges);

        Assert.That(registeredFrame, Is.Not.Null);
        Assert.That(registeredFrame, Is.TypeOf<FakeChangesFrame>());
    }

    [Test]
    public void CopyModelShardTest()
    {
        var stringValue = "test";
        var intValue = 42;
        var model = new FakeModel(new[] { new FakeModelShard() });
        FirstEntity? first = null;
        SecondEntity? second = null;
        new DelegateCommand<FakeModel>(model, m =>
        {
            var shard = m.Shard<IMutableFakeModelShard>();
            first = shard.FirstCollection.Add(new() { NonNullableStringProperty = stringValue });
            second = shard.SecondCollection.Add(new() { IntProperty = intValue });
            shard.OneToOneRelation.Add(first, second);
        }).Execute();

        var shard = model.Shard<IFakeModelShard>();
        var firstProperies = shard.FirstCollection.Get(first!);
        var secondProperies = shard.SecondCollection.Get(second!);

        var copy = (model.Shard<IFakeModelShard>() as ICopy<IModelShard>)?.Copy() as IFakeModelShard;

        Assert.That(copy, Is.Not.Null);
        Assert.That(copy, Is.TypeOf<FakeModelShard>());
        Assert.IsFalse(ReferenceEquals(shard, copy));
        Assert.That(copy.FirstCollection.First(), Is.EqualTo(first));
        Assert.That(copy.SecondCollection.First(), Is.EqualTo(second));
        Assert.That(copy.OneToOneRelation.Children(first!), Is.EqualTo(new[] { second }));
        Assert.That(copy.OneToOneRelation.Parents(second!), Is.EqualTo(new[] { first }));
        Assert.IsTrue(ReferenceEquals(copy.FirstCollection.Get(first!), firstProperies));
        Assert.That(copy.FirstCollection.Get(first!).NonNullableStringProperty, Is.EqualTo(stringValue));
        Assert.IsTrue(ReferenceEquals(copy.SecondCollection.Get(second!), secondProperies));
        Assert.That(copy.SecondCollection.Get(second!).IntProperty, Is.EqualTo(intValue));
    }
}
