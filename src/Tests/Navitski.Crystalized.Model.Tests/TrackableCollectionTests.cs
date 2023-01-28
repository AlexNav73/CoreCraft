using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using System.Collections;

namespace Navitski.Crystalized.Model.Tests;

public class TrackableCollectionTests
{
    private IMutableCollection<FirstEntity, FirstEntityProperties>? _collection;
    private ICollectionChangeSet<FirstEntity, FirstEntityProperties>? _changes;
    private IMutableCollection<FirstEntity, FirstEntityProperties>? _trackable;

    [SetUp]
    public void Setup()
    {
        _collection = A.Fake<IMutableCollection<FirstEntity, FirstEntityProperties>>();

        _changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>();
        _trackable = new TrackableCollection<FirstEntity, FirstEntityProperties>(_changes, _collection);
    }

    [Test]
    public void TrackableAddToCollectionTest()
    {
        _trackable!.Add(new());
        var change = _changes!.Single();

        A.CallTo(() => _collection!.Add(A<FirstEntityProperties>.Ignored)).MustHaveHappened();
        Assert.That(_changes!.HasChanges(), Is.True);
        Assert.That(change.Action, Is.EqualTo(CollectionAction.Add));
        Assert.That(change.Entity, Is.Not.Null);
        Assert.That(change.NewData, Is.Not.Null);
        Assert.That(change.OldData, Is.Null);
    }

    [Test]
    public void TrackableAddGuidInitToCollectionTest()
    {
        var entityId = Guid.NewGuid();
        A.CallTo(() => _collection!.Add(entityId, A<Func<FirstEntityProperties, FirstEntityProperties>>.Ignored)).Returns(new FirstEntity(entityId));
        
        _trackable!.Add(entityId, props => props);
        var change = _changes!.Single();

        A.CallTo(() => _collection!.Add(entityId, A<Func<FirstEntityProperties, FirstEntityProperties>>.Ignored)).MustHaveHappened();
        A.CallTo(() => _collection!.Get(A<FirstEntity>.Ignored)).MustHaveHappened();
        
        Assert.That(_changes!.HasChanges(), Is.True);
        Assert.That(change.Action, Is.EqualTo(CollectionAction.Add));
        Assert.That(change.Entity.Id, Is.EqualTo(entityId));
        Assert.That(change.NewData, Is.Not.Null);
        Assert.That(change.OldData, Is.Null);
    }

    [Test]
    public void TrackableAddEntityAndPropertyToCollectionTest()
    {
        var entity = new FirstEntity();
        var props = new FirstEntityProperties();
        _trackable!.Add(entity, props);
        var change = _changes!.Single();

        A.CallTo(() => _collection!.Add(entity, A<FirstEntityProperties>.Ignored)).MustHaveHappened();

        Assert.That(_changes!.HasChanges(), Is.True);
        Assert.That(change.Action, Is.EqualTo(CollectionAction.Add));
        Assert.That(change.Entity, Is.EqualTo(entity));
        Assert.That(change.NewData, Is.EqualTo(props));
        Assert.That(change.OldData, Is.Null);
    }

    [Test]
    public void TrackableCollectionCountTest()
    {
        A.CallTo(() => _collection!.Count).Returns(1);

        Assert.That(_trackable!.Count, Is.EqualTo(1));
    }

    [Test]
    public void TrackableCollectionGetTest()
    {
        var props = new FirstEntityProperties();
        A.CallTo(() => _collection!.Get(A<FirstEntity>.Ignored)).Returns(props);

        Assert.That(_trackable!.Get(new FirstEntity()), Is.EqualTo(props));
    }

    [Test]
    public void TrackableCollectionContainsTest()
    {
        var entity = new FirstEntity();
        
        _collection!.Contains(entity);

        A.CallTo(() => _collection!.Contains(entity)).MustHaveHappened();
    }

    [Test]
    public void TrackableRemoveFromCollectionTest()
    {
        A.CallTo(() => _collection!.Get(A<FirstEntity>.Ignored)).Returns(new FirstEntityProperties());

        _trackable!.Remove(A.Dummy<FirstEntity>());

        var change = _changes!.Single();

        A.CallTo(() => _collection!.Remove(A<FirstEntity>.Ignored)).MustHaveHappened();
        Assert.That(_changes!.HasChanges(), Is.True);
        Assert.That(change.Action, Is.EqualTo(CollectionAction.Remove));
        Assert.That(change.Entity, Is.Not.Null);
        Assert.That(change.NewData, Is.Null);
        Assert.That(change.OldData, Is.Not.Null);
    }

    [Test]
    public void TrackableModifyEntityInsideCollectionTest()
    {
        A.CallTo(() => _collection!.Get(A<FirstEntity>.Ignored))
            .ReturnsNextFromSequence(
                new FirstEntityProperties(),
                new FirstEntityProperties() { NullableStringProperty = "test" }
            );

        _trackable!.Modify(A.Dummy<FirstEntity>(), p => p with { NullableStringProperty = "test" });

        var change = _changes!.Single();

        A.CallTo(() => _collection!.Modify(A<FirstEntity>.Ignored, A<Func<FirstEntityProperties, FirstEntityProperties>>.Ignored))
            .MustHaveHappened();
        Assert.That(_changes!.HasChanges(), Is.True);
        Assert.That(change.Action, Is.EqualTo(CollectionAction.Modify));
        Assert.That(change.Entity, Is.Not.Null);
        Assert.That(change.NewData, Is.Not.Null);
        Assert.That(change.OldData, Is.Not.Null);
    }

    [Test]
    public void TrackableCollectionCopyTest()
    {
        Assert.Throws<InvalidOperationException>(() => _trackable!.Copy());
    }

    [Test]
    public void TrackableCollectionGetEnumeratorTest()
    {
        var enumerator = _trackable!.GetEnumerator();

        A.CallTo(() => _collection!.GetEnumerator()).MustHaveHappened();
    }

    [Test]
    public void TrackableCollectionGetEnumerator2Test()
    {
        var enumerator = ((IEnumerable)_trackable!).GetEnumerator();

        A.CallTo(() => _collection!.GetEnumerator()).MustHaveHappened();
    }

    [Test]
    public void TrackableCollectionPairsEnumeratorTest()
    {
        var enumerator = _trackable!.Pairs();

        A.CallTo(() => _collection!.Pairs()).MustHaveHappened();
    }
}
