using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Tests;

public class ModelChangesTests
{
    [Test]
    public void ChangesFrameRegisteredByConcreteTypeAndRetievedByInterfaceTypeTest()
    {
        var modelChanges = new ModelChanges();
        var changesFrame = modelChanges.Register(new FakeChangesFrame());

        var success = modelChanges.TryGetFrame<IFakeChangesFrame>(out var frame);

        Assert.That(success, Is.True);
        Assert.That(frame, Is.Not.Null);
        Assert.That(ReferenceEquals(changesFrame, frame), Is.True);
    }

    [Test]
    public void RegisterChangesFrameMultipleTimesTest()
    {
        var modelChanges = new ModelChanges();
        var changesFrame1 = modelChanges.Register(new FakeChangesFrame());
        var changesFrame2 = modelChanges.Register(new FakeChangesFrame());

        Assert.That(ReferenceEquals(changesFrame1, changesFrame2), Is.True);
    }

    [Test]
    public void InvertChangesFrameTest()
    {
        var modelChanges = new ModelChanges();
        var changesFrame = modelChanges.Register(new FakeChangesFrame());
        var entity = new FirstEntity();
        var props = new FirstEntityProperties();
        var value = "test";

        changesFrame.FirstCollection.Add(CollectionAction.Add, entity, props, props with { NonNullableStringProperty = value });
        var inverted = changesFrame.Invert();
        var change = ((IFakeChangesFrame)inverted).FirstCollection.SingleOrDefault();

        Assert.That(change, Is.Not.Null);
        Assert.That(change!.Action, Is.EqualTo(CollectionAction.Remove));
        Assert.That(change.Entity, Is.EqualTo(entity));
        Assert.That(change.OldData!.NonNullableStringProperty, Is.EqualTo(value));
        Assert.That(change.NewData, Is.EqualTo(props));
    }

    [Test]
    public void ApplyChangesTest()
    {
        var modelChanges = new ModelChanges();
        var changesFrame = modelChanges.Register(new FakeChangesFrame());
        var entity = new FirstEntity();
        var props = new FirstEntityProperties();
        var value = "test";
        var model = new Engine.Core.Model(new[] { new FakeModelShard() });

        changesFrame.FirstCollection.Add(CollectionAction.Add, entity, props, props with { NonNullableStringProperty = value });
        changesFrame.Apply(model);

        var shard = model.Shard<IFakeModelShard>();

        Assert.That(shard.FirstCollection.Count, Is.EqualTo(1));
        Assert.That(shard.FirstCollection.First(), Is.EqualTo(entity));
        Assert.That(shard.FirstCollection.Get(shard.FirstCollection.First()).NonNullableStringProperty, Is.EqualTo(value));
    }

    [Test]
    public void HasChangesTest()
    {
        var modelChanges = new ModelChanges();
        var changesFrame = modelChanges.Register(new FakeChangesFrame());
        var entity = new FirstEntity();
        var props = new FirstEntityProperties();
        var value = "test";

        changesFrame.FirstCollection.Add(CollectionAction.Add, entity, props, props with { NonNullableStringProperty = value });

        Assert.That(changesFrame.HasChanges(), Is.True);
    }
}
