using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Subscription;
using Navitski.Crystalized.Model.Engine.Subscription.Binding;

namespace Navitski.Crystalized.Model.Tests;

internal class CollectionSubscriptionTests
{
    [Test]
    public void BindingShouldBeDroppedWhenBoundedObjectDeletedTest()
    {
        var collectionSubscription = new CollectionSubscription<IFakeChangesFrame, FirstEntity, FirstEntityProperties>(x => x.FirstCollection);
        var onEntityChanged1Called = false;
        var onEntityChanged2Called = false;

        var firstEntity = BindToEntity(collectionSubscription, () => onEntityChanged1Called = true);
        var secondItem = CreateEntityBinding(() => onEntityChanged2Called = true);
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
    public void ItShouldBePossibleToHaveMultipleBindingsToTheSameEntityTest()
    {
        var collectionSubscription = new CollectionSubscription<IFakeChangesFrame, FirstEntity, FirstEntityProperties>(x => x.FirstCollection);

        var entity = new FirstEntity();
        var firstBinding = A.Fake<IEntityBinding<FirstEntity, FirstEntityProperties>>();
        var secondBinding = A.Fake<IEntityBinding<FirstEntity, FirstEntityProperties>>();

        collectionSubscription.Bind(entity, firstBinding);
        collectionSubscription.Bind(entity, secondBinding);

        collectionSubscription.Publish(CreateChanges(entity));

        A.CallTo(() => firstBinding.OnEntityChanged(A<FirstEntityProperties>.Ignored, A<FirstEntityProperties>.Ignored)).MustHaveHappenedOnceExactly();
        A.CallTo(() => secondBinding.OnEntityChanged(A<FirstEntityProperties>.Ignored, A<FirstEntityProperties>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Test]
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
    public void EntityBindingShouldBeRemovedIfEntityWasDeletedTest()
    {
        var collectionSubscription = new CollectionSubscription<IFakeChangesFrame, FirstEntity, FirstEntityProperties>(x => x.FirstCollection);
        var onEntityChanged1Called = false;
        var onEntityChanged2Called = false;

        var entityBinding = CreateEntityBinding(() => onEntityChanged1Called = !onEntityChanged1Called);
        var entity = new FirstEntity();
        var collectionBinding = CreateCollectionBinding(() => onEntityChanged2Called = true);

        collectionSubscription.Bind(entity, entityBinding);
        collectionSubscription.Bind(collectionBinding);

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
        var binding = CreateCollectionBinding(action);

        collectionSubscription.Bind(binding);

        collectionSubscription.Publish(CreateChanges(new()));

        // binding is dropped here
        binding = null;
    }

    private FirstEntity BindToEntity(
        CollectionSubscription<IFakeChangesFrame, FirstEntity, FirstEntityProperties> collectionSubscription,
        Action action)
    {
        var binding = CreateEntityBinding(action);
        var entity = new FirstEntity();

        collectionSubscription.Bind(entity, binding);

        // binding is dropped here
        binding = null;
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

    private IEntityBinding<FirstEntity, FirstEntityProperties> CreateEntityBinding(Action action)
    {
        var binding = A.Fake<IEntityBinding<FirstEntity, FirstEntityProperties>>();

        A.CallTo(() => binding.OnEntityChanged(A<FirstEntityProperties>.Ignored, A<FirstEntityProperties>.Ignored)).Invokes(action);

        return binding;
    }

    private ICollectionBinding<FirstEntity, FirstEntityProperties> CreateCollectionBinding(Action action)
    {
        var binding = A.Fake<ICollectionBinding<FirstEntity, FirstEntityProperties>>();

        A.CallTo(() => binding.OnCollectionChanged(A<BindingChanges<FirstEntity, FirstEntityProperties>>.Ignored)).Invokes(action);

        return binding;
    }
}
