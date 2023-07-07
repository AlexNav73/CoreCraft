using CoreCraft.Engine.ChangesTracking;
using CoreCraft.Engine.Core;
using CoreCraft.Engine.Subscription;

namespace CoreCraft.Tests.Engine.Subscription;

internal class CollectionSubscriptionTests
{
    [Test]
    [Ignore("Unstable. GC doesn't reclaim memory so the binding is not deleted and subscription lifetime is longer than necessary")]
    public void BindingShouldBeDroppedWhenBoundedObjectDeletedTest()
    {
        var collectionSubscription = new CollectionSubscription<IFakeChangesFrame, FirstEntity, FirstEntityProperties>(x => x.FirstCollection);
        var onEntityChanged1Called = false;
        var onEntityChanged2Called = false;

        var firstEntity = BindToEntity(collectionSubscription, () => onEntityChanged1Called = true);
        var secondItem = CreateEntityObserver(() => onEntityChanged2Called = true);
        var secondEntity = new FirstEntity();

        collectionSubscription.Bind(secondEntity, secondItem);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        collectionSubscription.Publish(CreateChanges(firstEntity));
        collectionSubscription.Publish(CreateChanges(secondEntity));

        Assert.That(onEntityChanged1Called, Is.False);
        Assert.That(onEntityChanged2Called, Is.True);
    }

    [Test]
    [Ignore("Unstable. GC doesn't reclaim memory so the binding is not deleted and subscription lifetime is longer than necessary")]
    public void CallingDisposeOnSubscriptionWillRemoveEntityBindingTest()
    {
        var collectionSubscription = new CollectionSubscription<IFakeChangesFrame, FirstEntity, FirstEntityProperties>(x => x.FirstCollection);
        var onEntityChanged1Called = false;
        var onEntityChanged2Called = false;

        var entity = new FirstEntity();
        var firstItem = CreateEntityObserver(() => onEntityChanged1Called = true);
        var secondItem = CreateEntityObserver(() => onEntityChanged2Called = true);

        var subscription1 = collectionSubscription.Bind(entity, firstItem);
        var subscription2 = collectionSubscription.Bind(entity, secondItem);

        collectionSubscription.Publish(CreateChanges(entity));

        Assert.That(onEntityChanged1Called, Is.True);
        Assert.That(onEntityChanged2Called, Is.True);

        onEntityChanged1Called = false;
        onEntityChanged2Called = false;

        subscription1.Dispose();

        collectionSubscription.Publish(CreateChanges(entity));

        Assert.That(onEntityChanged1Called, Is.False);
        Assert.That(onEntityChanged2Called, Is.True);

        subscription2.Dispose();

        onEntityChanged2Called = false;

        collectionSubscription.Publish(CreateChanges(entity));

        Assert.That(onEntityChanged2Called, Is.False);
    }

    [Test]
    public void ItShouldBePossibleToHaveMultipleBindingsToTheSameEntityTest()
    {
        var collectionSubscription = new CollectionSubscription<IFakeChangesFrame, FirstEntity, FirstEntityProperties>(x => x.FirstCollection);

        var entity = new FirstEntity();
        var firstObserver = A.Fake<IObserver<IEntityChange<FirstEntity, FirstEntityProperties>>>();
        var secondObserver = A.Fake<IObserver<IEntityChange<FirstEntity, FirstEntityProperties>>>();

        collectionSubscription.Bind(entity, firstObserver);
        collectionSubscription.Bind(entity, secondObserver);

        collectionSubscription.Publish(CreateChanges(entity));

        A.CallTo(() => firstObserver.OnNext(A<IEntityChange<FirstEntity, FirstEntityProperties>>.Ignored)).MustHaveHappenedOnceExactly();
        A.CallTo(() => secondObserver.OnNext(A<IEntityChange<FirstEntity, FirstEntityProperties>>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Test]
    [Ignore("Unstable. GC doesn't reclaim memory so the binding is not deleted and subscription lifetime is longer than necessary")]
    public void CollectionBindingShouldBeDroppedWhenBoundedObjectDeletedTest()
    {
        var collectionSubscription = new CollectionSubscription<IFakeChangesFrame, FirstEntity, FirstEntityProperties>(x => x.FirstCollection);
        var onEntityChanged1Called = false;

        BindToCollectionAndRiseEventBeforeBindingIsDeleted(collectionSubscription, () => onEntityChanged1Called = !onEntityChanged1Called);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        collectionSubscription.Publish(CreateChanges(new()));

        Assert.That(onEntityChanged1Called, Is.True);
    }

    [Test]
    [Ignore("Unstable. GC doesn't reclaim memory so the binding is not deleted and subscription lifetime is longer than necessary")]
    public void EntityBindingShouldBeRemovedIfEntityWasDeletedTest()
    {
        var collectionSubscription = new CollectionSubscription<IFakeChangesFrame, FirstEntity, FirstEntityProperties>(x => x.FirstCollection);
        var onEntityChanged1Called = false;
        var onEntityChanged2Called = false;

        var entityObserver = CreateEntityObserver(() => onEntityChanged1Called = !onEntityChanged1Called);
        var entity = new FirstEntity();
        var collectionObserver = CreateCollectionChangeObserver(() => onEntityChanged2Called = true);

        collectionSubscription.Bind(entity, entityObserver);
        collectionSubscription.Bind(collectionObserver);

        collectionSubscription.Publish(CreateChanges(entity));
        collectionSubscription.Publish(CreateChanges(entity, CollectionAction.Remove));
        collectionSubscription.Publish(CreateChanges(entity));

        Assert.That(onEntityChanged1Called, Is.True);
        Assert.That(onEntityChanged2Called, Is.True);
    }

    private void BindToCollectionAndRiseEventBeforeBindingIsDeleted(
        CollectionSubscription<IFakeChangesFrame, FirstEntity, FirstEntityProperties> collectionSubscription,
        Action action)
    {
        var observer = CreateCollectionChangeObserver(action);

        collectionSubscription.Bind(observer);

        collectionSubscription.Publish(CreateChanges(new()));

        // observer is dropped here
        observer = null;
    }

    private FirstEntity BindToEntity(
        CollectionSubscription<IFakeChangesFrame, FirstEntity, FirstEntityProperties> collectionSubscription,
        Action action)
    {
        var observer = CreateEntityObserver(action);
        var entity = new FirstEntity();

        collectionSubscription.Bind(entity, observer);

        // observer is dropped here
        observer = null;
        return entity;
    }

    private Change<IFakeChangesFrame> CreateChanges(FirstEntity entity, CollectionAction collectionAction = CollectionAction.Modify)
    {
        var change = A.Fake<ICollectionChange<FirstEntity, FirstEntityProperties>>();
        var collectionChangeSet = A.Fake<ICollectionChangeSet<FirstEntity, FirstEntityProperties>>();
        var changesFrame = A.Fake<IFakeChangesFrame>();

        A.CallTo(() => change.Action).Returns(collectionAction);
        A.CallTo(() => change.Entity).Returns(entity);
        A.CallTo(() => collectionChangeSet.HasChanges()).Returns(true);
        A.CallTo(() => collectionChangeSet.GetEnumerator()).Returns(new List<ICollectionChange<FirstEntity, FirstEntityProperties>> { change }.GetEnumerator());
        A.CallTo(() => changesFrame.FirstCollection).Returns(collectionChangeSet);

        return new Change<IFakeChangesFrame>(A.Fake<IModel>(), A.Fake<IModel>(), changesFrame);
    }

    private IObserver<IEntityChange<FirstEntity, FirstEntityProperties>> CreateEntityObserver(Action action)
    {
        var observer = A.Fake<IObserver<IEntityChange<FirstEntity, FirstEntityProperties>>>();

        A.CallTo(() => observer.OnNext(A<IEntityChange<FirstEntity, FirstEntityProperties>>.Ignored)).Invokes(action);

        return observer;
    }

    private IObserver<BindingChanges<FirstEntity, FirstEntityProperties>> CreateCollectionChangeObserver(Action action)
    {
        var observer = A.Fake<IObserver<BindingChanges<FirstEntity, FirstEntityProperties>>>();

        A.CallTo(() => observer.OnNext(A<BindingChanges<FirstEntity, FirstEntityProperties>>.Ignored)).Invokes(action);

        return observer;
    }
}
