using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
#if NETCOREAPP3_1 || NET48_OR_GREATER
using Navitski.Crystalized.Model.Engine.Exceptions;
#endif
using Navitski.Crystalized.Model.Engine.Subscription;

namespace Navitski.Crystalized.Model.Tests.Engine.Subscription;

internal class ModelShardSubscriptionTests
{
    [Test]
    public void FirstCallOfWithCollectionWillCreateNewSubscriptionTest()
    {
        var subscription = new ModelShardSubscription<IFakeChangesFrame>();

        var collectionSubscription = subscription.With(x => x.FirstCollection);

        Assert.That(collectionSubscription, Is.Not.Null);
    }

    [Test]
    public void SecondCallOfWithCollectionWillReturnTheSameSubscriptionTest()
    {
        var subscription = new ModelShardSubscription<IFakeChangesFrame>();

        var collectionSubscription1 = subscription.With(x => x.FirstCollection);
        var collectionSubscription2 = subscription.With(x => x.FirstCollection);

        Assert.That(collectionSubscription1, Is.Not.Null);
        Assert.That(collectionSubscription2, Is.Not.Null);
        Assert.That(ReferenceEquals(collectionSubscription1, collectionSubscription2), Is.True);
    }

    [Test]
    public void LambdaArgumentNameShouldNotBeTheSameToReturnTheSameCollectionSubscriptionTest()
    {
        var subscription = new ModelShardSubscription<IFakeChangesFrame>();

        var collectionSubscription1 = subscription.With(x => x.FirstCollection);
        var collectionSubscription2 = subscription.With(y => y.FirstCollection);
        var collectionSubscription3 = subscription.With(name => name.FirstCollection);
        var collectionSubscription4 = subscription.With(nameOfVar => nameOfVar.FirstCollection);
        var collectionSubscription5 = subscription.With(name_of_var => name_of_var.FirstCollection);

        Assert.That(collectionSubscription1, Is.Not.Null);
        Assert.That(collectionSubscription2, Is.Not.Null);
        Assert.That(collectionSubscription3, Is.Not.Null);
        Assert.That(collectionSubscription4, Is.Not.Null);
        Assert.That(collectionSubscription5, Is.Not.Null);
        Assert.That(ReferenceEquals(collectionSubscription1, collectionSubscription2), Is.True);
        Assert.That(ReferenceEquals(collectionSubscription2, collectionSubscription3), Is.True);
        Assert.That(ReferenceEquals(collectionSubscription3, collectionSubscription4), Is.True);
        Assert.That(ReferenceEquals(collectionSubscription4, collectionSubscription5), Is.True);
    }

    [Test]
    public void LambdaArgumentNameShouldNotBeTheSameToReturnTheSameRelationSubscriptionTest()
    {
        var subscription = new ModelShardSubscription<IFakeChangesFrame>();

        var relationSubscription1 = subscription.With(x => x.OneToOneRelation);
        var relationSubscription2 = subscription.With(y => y.OneToOneRelation);
        var relationSubscription3 = subscription.With(name => name.OneToOneRelation);
        var relationSubscription4 = subscription.With(nameOfVar => nameOfVar.OneToOneRelation);
        var relationSubscription5 = subscription.With(name_of_var => name_of_var.OneToOneRelation);

        Assert.That(relationSubscription1, Is.Not.Null);
        Assert.That(relationSubscription2, Is.Not.Null);
        Assert.That(relationSubscription3, Is.Not.Null);
        Assert.That(relationSubscription4, Is.Not.Null);
        Assert.That(relationSubscription5, Is.Not.Null);
        Assert.That(ReferenceEquals(relationSubscription1, relationSubscription2), Is.True);
        Assert.That(ReferenceEquals(relationSubscription2, relationSubscription3), Is.True);
        Assert.That(ReferenceEquals(relationSubscription3, relationSubscription4), Is.True);
        Assert.That(ReferenceEquals(relationSubscription4, relationSubscription5), Is.True);
    }

    [Test]
    public void SubscriptionToInvalidCollectionOrRelationShouldThrowExceptionTest()
    {
        var subscription = new ModelShardSubscription<IFakeChangesFrame>();
        var fakeCollection = A.Fake<ICollectionChangeSet<FirstEntity, FirstEntityProperties>>();
        var fakeRelation = A.Fake<IRelationChangeSet<FirstEntity, SecondEntity>>();

        Assert.Throws<InvalidOperationException>(() => subscription.With(x => fakeCollection));
        Assert.Throws<InvalidOperationException>(() => subscription.With(y => fakeRelation));
    }

    [Test]
    public void FirstCallOfWithRelationWillCreateNewSubscriptionTest()
    {
        var subscription = new ModelShardSubscription<IFakeChangesFrame>();

        var relationSubscription = subscription.With(x => x.OneToOneRelation);

        Assert.That(relationSubscription, Is.Not.Null);
    }

    [Test]
    public void SecondCallOfWithRelationWillReturnTheSameSubscriptionTest()
    {
        var subscription = new ModelShardSubscription<IFakeChangesFrame>();

        var relationSubscription1 = subscription.With(x => x.OneToOneRelation);
        var relationSubscription2 = subscription.With(x => x.OneToOneRelation);

        Assert.That(relationSubscription1, Is.Not.Null);
        Assert.That(relationSubscription2, Is.Not.Null);
        Assert.That(ReferenceEquals(relationSubscription1, relationSubscription2), Is.True);
    }

    [Test]
    public void PushChangesWillNotifySubscriptionsTest()
    {
        var subscription = new ModelShardSubscription<IFakeChangesFrame>();
        var modelChanges = CreateFakeModelChanges();

        var subscriptionWasCalled = false;
        var disposable = subscription.Subscribe(new AnonymousObserver<Change<IFakeChangesFrame>>(m => subscriptionWasCalled = true));

        subscription.Publish(new Change<IModelChanges>(A.Fake<IModel>(), A.Fake<IModel>(), modelChanges));

        Assert.That(disposable, Is.Not.Null);
        Assert.That(subscriptionWasCalled, Is.True);
    }

    [Test]
    public void PushChangesWillNotNotifySubscriptionsWhenNoChangesTest()
    {
        var subscription = new ModelShardSubscription<IFakeChangesFrame>();
        var modelChanges = CreateFakeModelChanges(frameHasChanges: false);

        var subscriptionWasCalled = false;
        var disposable = subscription.Subscribe(new AnonymousObserver<Change<IFakeChangesFrame>>(m => subscriptionWasCalled = true));

        subscription.Publish(new Change<IModelChanges>(A.Fake<IModel>(), A.Fake<IModel>(), modelChanges));

        Assert.That(disposable, Is.Not.Null);
        Assert.That(subscriptionWasCalled, Is.False);
    }

    [Test]
    public void PushChangesWillNotNotifySubscriptionsWhenMissingChangesFrameTest()
    {
        var subscription = new ModelShardSubscription<IFakeChangesFrame>();
        var modelChanges = CreateFakeModelChanges(hasChangesFrame: false);

        var subscriptionWasCalled = false;
        var disposable = subscription.Subscribe(new AnonymousObserver<Change<IFakeChangesFrame>>(m => subscriptionWasCalled = true));

        subscription.Publish(new Change<IModelChanges>(A.Fake<IModel>(), A.Fake<IModel>(), modelChanges));

        Assert.That(disposable, Is.Not.Null);
        Assert.That(subscriptionWasCalled, Is.False);
    }

    [Test]
    public void PushChangesWillNotifyCollectionSubscriptionsTest()
    {
        var subscription = new ModelShardSubscription<IFakeChangesFrame>();

        var subscriptionWasCalled = false;
        var disposable = subscription.With(x => x.FirstCollection).Subscribe(new AnonymousObserver<Change<ICollectionChangeSet<FirstEntity, FirstEntityProperties>>>(m => subscriptionWasCalled = true));
        var fakeCollectionChanges = A.Fake<ICollectionChangeSet<FirstEntity, FirstEntityProperties>>();
        var frame = A.Fake<IFakeChangesFrame>();
        var modelChanges = CreateFakeModelChanges(frame);

        A.CallTo(() => fakeCollectionChanges.HasChanges()).Returns(true);
        A.CallTo(() => frame.HasChanges()).Returns(true);
        A.CallTo(() => frame.FirstCollection).Returns(fakeCollectionChanges);

        subscription.Publish(new Change<IModelChanges>(A.Fake<IModel>(), A.Fake<IModel>(), modelChanges));

        Assert.That(disposable, Is.Not.Null);
        Assert.That(subscriptionWasCalled, Is.True);
    }

    [Test]
    public void PushChangesWillNotifyRelationSubscriptionsTest()
    {
        var subscription = new ModelShardSubscription<IFakeChangesFrame>();

        var subscriptionWasCalled = false;
        var disposable = subscription.With(x => x.OneToOneRelation).Subscribe(new AnonymousObserver<Change<IRelationChangeSet<FirstEntity, SecondEntity>>>(m => subscriptionWasCalled = true));
        var fakeRelationChanges = A.Fake<IRelationChangeSet<FirstEntity, SecondEntity>>();
        var frame = A.Fake<IFakeChangesFrame>();
        var modelChanges = CreateFakeModelChanges(frame);

        A.CallTo(() => fakeRelationChanges.HasChanges()).Returns(true);
        A.CallTo(() => frame.HasChanges()).Returns(true);
        A.CallTo(() => frame.OneToOneRelation).Returns(fakeRelationChanges);

        subscription.Publish(new Change<IModelChanges>(A.Fake<IModel>(), A.Fake<IModel>(), modelChanges));

        Assert.That(disposable, Is.Not.Null);
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
