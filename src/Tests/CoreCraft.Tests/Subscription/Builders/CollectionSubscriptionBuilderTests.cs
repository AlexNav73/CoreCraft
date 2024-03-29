﻿using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Subscription;
using CoreCraft.Subscription.Builders;

namespace CoreCraft.Tests.Subscription.Builders;

internal class CollectionSubscriptionBuilderTests
{
    [TestCase(false, false)]
    [TestCase(true, true)]
    public void SubscribeTest(bool withChanges, bool handlerIsCalled)
    {
        var subscriptionBuilder = CreateTestee(withChanges);
        var handlerCalledDuringSubscription = false;

        subscriptionBuilder.Subscribe(x => handlerCalledDuringSubscription = true);

        Assert.That(handlerCalledDuringSubscription, Is.EqualTo(handlerIsCalled));
    }

    [TestCase(false, 0)]
    [TestCase(true, 1)]
    public void BindToEntityTest(bool withChanges, int handlerCalledTimes)
    {
        var subscriptionBuilder = CreateTestee(withChanges);
        var binding = A.Fake<IObserver<IEntityChange<FirstEntity, FirstEntityProperties>>>();
        var entity = new FirstEntity();

        subscriptionBuilder.Bind(entity, binding);

        A.CallTo(() => binding.OnNext(A<IEntityChange<FirstEntity, FirstEntityProperties>>.Ignored)).MustHaveHappened(handlerCalledTimes, Times.Exactly);
    }

    [TestCase(false, false)]
    [TestCase(true, true)]
    public void BindToEntityUsingExtensionMethodTest(bool withChanges, bool onNextCallIsExpected)
    {
        var subscriptionBuilder = CreateTestee(withChanges);
        var entity = new FirstEntity();
        var onNextWasCalled = false;

        subscriptionBuilder.Bind(entity, _ => onNextWasCalled = true);

        Assert.That(onNextWasCalled, Is.EqualTo(onNextCallIsExpected));
    }

    [TestCase(false, 0, CollectionAction.Modify)]
    [TestCase(true, 1, CollectionAction.Add)]
    [TestCase(true, 1, CollectionAction.Modify)]
    [TestCase(true, 1, CollectionAction.Remove)]
    public void BindToCollectionTest(bool withChanges, int handlerCalledTimes, CollectionAction action)
    {
        var subscriptionBuilder = CreateTestee(withChanges, action);
        var binding = A.Fake<IObserver<BindingChanges<FirstEntity, FirstEntityProperties>>>();

        subscriptionBuilder.Bind(binding);

        A.CallTo(() => binding.OnNext(A<BindingChanges<FirstEntity, FirstEntityProperties>>.Ignored)).MustHaveHappened(handlerCalledTimes, Times.Exactly);
    }

    [TestCase(false, false, CollectionAction.Modify)]
    [TestCase(true, true, CollectionAction.Add)]
    [TestCase(true, true, CollectionAction.Modify)]
    [TestCase(true, true, CollectionAction.Remove)]
    public void BindToCollectionTest(bool withChanges, bool onNextCallIsExpected, CollectionAction action)
    {
        var subscriptionBuilder = CreateTestee(withChanges, action);
        var onNextWasCalled = false;

        subscriptionBuilder.Bind(_ => onNextWasCalled = true);

        Assert.That(onNextWasCalled, Is.EqualTo(onNextCallIsExpected));
    }

    private CollectionSubscriptionBuilder<IFakeChangesFrame, FirstEntity, FirstEntityProperties> CreateTestee(bool withChanges, CollectionAction collectionAction = CollectionAction.Modify)
    {
        Change<IFakeChangesFrame>? changes = null;

        if (withChanges)
        {
            var changesFrame = A.Fake<IFakeChangesFrame>();
            var collection = A.Fake<ICollectionChangeSet<FirstEntity, FirstEntityProperties>>();
            var change = A.Fake<ICollectionChange<FirstEntity, FirstEntityProperties>>();

            A.CallTo(() => changesFrame.FirstCollection).Returns(collection);
            A.CallTo(() => change.Action).Returns(collectionAction);
            A.CallTo(() => collection.HasChanges()).Returns(true);
            A.CallTo(() => collection.GetEnumerator()).Returns(new List<ICollectionChange<FirstEntity, FirstEntityProperties>>() { change }.GetEnumerator());

            changes = new Change<IFakeChangesFrame>(A.Fake<IModel>(), A.Fake<IModel>(), changesFrame);
        }

        var subscription = new CollectionSubscription<IFakeChangesFrame, FirstEntity, FirstEntityProperties>(x => x.FirstCollection);
        var subscriptionBuilder = new CollectionSubscriptionBuilder<IFakeChangesFrame, FirstEntity, FirstEntityProperties>(subscription, changes);

        return subscriptionBuilder;
    }
}
