using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Exceptions;
using Navitski.Crystalized.Model.Engine.Subscription;

namespace Navitski.Crystalized.Model.Tests;

internal class ModelShardSubscriberTests
{
    [Test]
    public void FirstCallOfWithCollectionWillCreateNewSubscriberTest()
    {
        var subscriber = new ModelShardSubscriber<IFakeChangesFrame>();

        var collectionSubscriber = subscriber.With(x => x.FirstCollection);

        Assert.That(collectionSubscriber, Is.Not.Null);
    }

    [Test]
    public void SecondCallOfWithCollectionWillReturnTheSameSubscriberTest()
    {
        var subscriber = new ModelShardSubscriber<IFakeChangesFrame>();

        var collectionSubscriber1 = subscriber.With(x => x.FirstCollection);
        var collectionSubscriber2 = subscriber.With(x => x.FirstCollection);

        Assert.That(collectionSubscriber1, Is.Not.Null);
        Assert.That(collectionSubscriber2, Is.Not.Null);
        Assert.That(ReferenceEquals(collectionSubscriber1, collectionSubscriber2), Is.True);
    }

    [Test]
    public void CallOfWithCollectionWillThrowOnInvalidExpressionTest()
    {
        var subscriber = new ModelShardSubscriber<IFakeChangesFrame>();

        Assert.Throws<InvalidPropertySubscriptionException>(() => subscriber.With(x => new CollectionChangeSet<FirstEntity, FirstEntityProperties>()));
    }

    [Test]
    public void FirstCallOfWithRelationWillCreateNewSubscriberTest()
    {
        var subscriber = new ModelShardSubscriber<IFakeChangesFrame>();

        var relationSubscriber = subscriber.With(x => x.OneToOneRelation);

        Assert.That(relationSubscriber, Is.Not.Null);
    }

    [Test]
    public void SecondCallOfWithRelationWillReturnTheSameSubscriberTest()
    {
        var subscriber = new ModelShardSubscriber<IFakeChangesFrame>();

        var relationSubscriber1 = subscriber.With(x => x.OneToOneRelation);
        var relationSubscriber2 = subscriber.With(x => x.OneToOneRelation);

        Assert.That(relationSubscriber1, Is.Not.Null);
        Assert.That(relationSubscriber2, Is.Not.Null);
        Assert.That(ReferenceEquals(relationSubscriber1, relationSubscriber2), Is.True);
    }

    [Test]
    public void CallOfWithRelationWillThrowOnInvalidExpressionTest()
    {
        var subscriber = new ModelShardSubscriber<IFakeChangesFrame>();

        Assert.Throws<InvalidPropertySubscriptionException>(() => subscriber.With(x => new RelationChangeSet<FirstEntity, SecondEntity>()));
    }

    [Test]
    public void PushChangesWillNotifySubscriptionsTest()
    {
        var subscriber = new ModelShardSubscriber<IFakeChangesFrame>();
        IModelChanges modelChanges = CreateFakeModelChanges();

        var subscriptionWasCalled = false;
        var subscription = subscriber.Subscribe(m => subscriptionWasCalled = true);

        subscriber.Push(new Message<IModelChanges>(A.Fake<IModel>(), A.Fake<IModel>(), modelChanges));

        Assert.That(subscription, Is.Not.Null);
        Assert.That(subscriptionWasCalled, Is.True);
    }

    [Test]
    public void PushChangesWillNotNotifySubscriptionsWhenNoChangesTest()
    {
        var subscriber = new ModelShardSubscriber<IFakeChangesFrame>();
        IModelChanges modelChanges = CreateFakeModelChanges(frameHasChanges: false);

        var subscriptionWasCalled = false;
        var subscription = subscriber.Subscribe(m => subscriptionWasCalled = true);

        subscriber.Push(new Message<IModelChanges>(A.Fake<IModel>(), A.Fake<IModel>(), modelChanges));

        Assert.That(subscription, Is.Not.Null);
        Assert.That(subscriptionWasCalled, Is.False);
    }

    [Test]
    public void PushChangesWillNotNotifySubscriptionsWhenMissingChangesFrameTest()
    {
        var subscriber = new ModelShardSubscriber<IFakeChangesFrame>();
        IModelChanges modelChanges = CreateFakeModelChanges(hasChangesFrame: false);

        var subscriptionWasCalled = false;
        var subscription = subscriber.Subscribe(m => subscriptionWasCalled = true);

        subscriber.Push(new Message<IModelChanges>(A.Fake<IModel>(), A.Fake<IModel>(), modelChanges));

        Assert.That(subscription, Is.Not.Null);
        Assert.That(subscriptionWasCalled, Is.False);
    }

    [Test]
    public void PushChangesWillNotifyCollectionSubscriptionsTest()
    {
        var subscriber = new ModelShardSubscriber<IFakeChangesFrame>();

        var subscriptionWasCalled = false;
        var subscription = subscriber.With(x => x.FirstCollection).Subscribe(m => subscriptionWasCalled = true);
        var fakeCollectionChanges = A.Fake<ICollectionChangeSet<FirstEntity, FirstEntityProperties>>();
        var frame = A.Fake<IFakeChangesFrame>();
        var modelChanges = CreateFakeModelChanges(frame);

        A.CallTo(() => fakeCollectionChanges.HasChanges()).Returns(true);
        A.CallTo(() => frame.HasChanges()).Returns(true);
        A.CallTo(() => frame.FirstCollection).Returns(fakeCollectionChanges);

        subscriber.Push(new Message<IModelChanges>(A.Fake<IModel>(), A.Fake<IModel>(), modelChanges));

        Assert.That(subscription, Is.Not.Null);
        Assert.That(subscriptionWasCalled, Is.True);
    }

    [Test]
    public void PushChangesWillNotifyRelationSubscriptionsTest()
    {
        var subscriber = new ModelShardSubscriber<IFakeChangesFrame>();

        var subscriptionWasCalled = false;
        var subscription = subscriber.With(x => x.OneToOneRelation).Subscribe(m => subscriptionWasCalled = true);
        var fakeRelationChanges = A.Fake<IRelationChangeSet<FirstEntity, SecondEntity>>();
        var frame = A.Fake<IFakeChangesFrame>();
        var modelChanges = CreateFakeModelChanges(frame);

        A.CallTo(() => fakeRelationChanges.HasChanges()).Returns(true);
        A.CallTo(() => frame.HasChanges()).Returns(true);
        A.CallTo(() => frame.OneToOneRelation).Returns(fakeRelationChanges);

        subscriber.Push(new Message<IModelChanges>(A.Fake<IModel>(), A.Fake<IModel>(), modelChanges));

        Assert.That(subscription, Is.Not.Null);
        Assert.That(subscriptionWasCalled, Is.True);
    }

    private static IModelChanges CreateFakeModelChanges(
        IFakeChangesFrame? f = null,
        bool hasChangesFrame = true,
        bool frameHasChanges = true)
    {
        var modelChanges = A.Fake<IModelChanges>();
        IFakeChangesFrame? ignore = null;
        var frame = f ?? A.Fake<IFakeChangesFrame>();

        A.CallTo(() => frame.HasChanges()).Returns(frameHasChanges);
        A.CallTo(() => modelChanges.TryGetFrame<IFakeChangesFrame>(out ignore))
            .Returns(hasChangesFrame)
            .AssignsOutAndRefParameters(frame);

        return modelChanges;
    }
}
