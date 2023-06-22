using System.Collections;
using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Features;

namespace Navitski.Crystalized.Model.Tests.Engine.ChangesTracking;

public class ModelChangesTests
{
    [Test]
    public void ChangesFrameRegisteredByConcreteTypeAndRetrievedByInterfaceTypeTest()
    {
        var modelChanges = new ModelChanges();
        var changesFrame = modelChanges.Register(() => new FakeChangesFrame());

        var success = modelChanges.TryGetFrame<IFakeChangesFrame>(out var frame);

        Assert.That(success, Is.True);
        Assert.That(frame, Is.Not.Null);
        Assert.That(ReferenceEquals(changesFrame, frame), Is.True);
    }

    [Test]
    public void RegisterChangesFrameMultipleTimesTest()
    {
        var modelChanges = new ModelChanges();
        var changesFrame1 = modelChanges.Register(() => new FakeChangesFrame());
        var changesFrame2 = modelChanges.Register(() => new FakeChangesFrame());

        Assert.That(ReferenceEquals(changesFrame1, changesFrame2), Is.True);
    }

    [Test]
    public void InvertChangesFrameTest()
    {
        var changesFrame = new FakeChangesFrame();
        var entity = new FirstEntity();
        var props = new FirstEntityProperties();
        var value = "test";

        changesFrame.FirstCollection.Add(CollectionAction.Add, entity, props, props with { NonNullableStringProperty = value });
        var inverted = ((IChangesFrame)changesFrame).Invert();
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
        var changesFrame = new FakeChangesFrame();
        var entity = new FirstEntity();
        var props = new FirstEntityProperties();
        var value = "test";
        var model = new Model.Engine.Core.Model(new[] { new FakeModelShard() });
        var snapshot = new Snapshot(model, new[] { new CoWFeature() });

        changesFrame.FirstCollection.Add(CollectionAction.Add, entity, props, props with { NonNullableStringProperty = value });
        changesFrame.Apply(snapshot);

        var shard = snapshot.ToModel().Shard<IFakeModelShard>();

        Assert.That(shard.FirstCollection.Count, Is.EqualTo(1));
        Assert.That(shard.FirstCollection.First(), Is.EqualTo(entity));
        Assert.That(shard.FirstCollection.Get(shard.FirstCollection.First()).NonNullableStringProperty, Is.EqualTo(value));
    }

    [Test]
    public void MigrateTest()
    {
        var modelChanges = new ModelChanges();
        var changesFrame = modelChanges.Register(() => new FakeChangesFrame());
        var entity = new FirstEntity();
        var props = new FirstEntityProperties();

        changesFrame.FirstCollection.Add(CollectionAction.Add, entity, props, props with { NonNullableStringProperty = "test" });

        var modelChanges2 = new ModelChanges();
        var changesFrame2 = modelChanges2.Register(() => new FakeChangesFrame());
        var props2 = new FirstEntityProperties();

        changesFrame2.FirstCollection.Add(CollectionAction.Remove, entity, props2, props2 with { NonNullableStringProperty = "test" });

        var merged = modelChanges.Merge(modelChanges2);

        Assert.That(merged.HasChanges(), Is.False);
    }

    [Test]
    public void HasChangesTest()
    {
        var modelChanges = new ModelChanges();
        var changesFrame = modelChanges.Register(() => new FakeChangesFrame());
        var entity = new FirstEntity();
        var props = new FirstEntityProperties();
        var value = "test";

        changesFrame.FirstCollection.Add(CollectionAction.Add, entity, props, props with { NonNullableStringProperty = value });

        Assert.That(changesFrame.HasChanges(), Is.True);
    }

    [Test]
    public void GetEnumeratorTest()
    {
        var modelChanges = new ModelChanges();

        Assert.That(modelChanges.GetEnumerator(), Is.Not.Null);
    }

    [Test]
    public void GetNonGenericEnumeratorTest()
    {
        var modelChanges = new ModelChanges();

        Assert.That(((IEnumerable)modelChanges).GetEnumerator(), Is.Not.Null);
    }
}
