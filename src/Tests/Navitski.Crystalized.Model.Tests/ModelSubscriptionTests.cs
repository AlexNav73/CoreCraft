using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Subscription;

namespace Navitski.Crystalized.Model.Tests;

internal class ModelSubscriptionTests
{
    [Test]
    public void FirstCallOfToWillCreateNewSubscriptionTest()
    {
        var subscription = new ModelSubscription();

        var shardSubscription = subscription.GetOrCreateSubscriptionFor<IFakeChangesFrame>();

        Assert.That(shardSubscription, Is.Not.Null);
    }

    [Test]
    public void SecondCallOfToWillReturnTheSameSubscriptionTest()
    {
        var subscription = new ModelSubscription();

        var shardSubscription1 = subscription.GetOrCreateSubscriptionFor<IFakeChangesFrame>();
        var shardSubscription2 = subscription.GetOrCreateSubscriptionFor<IFakeChangesFrame>();

        Assert.That(shardSubscription1, Is.Not.Null);
        Assert.That(shardSubscription2, Is.Not.Null);
        Assert.That(ReferenceEquals(shardSubscription1, shardSubscription2), Is.True);
    }

    [Test]
    public void PublishChangesWillNotifySubscriptionsTest()
    {
        var subscription = new ModelSubscription();

        var subscriptionWasCalled = false;
        var disposable = subscription.Add(m => subscriptionWasCalled = true);

        subscription.Publish(new Change<IModelChanges>(A.Fake<IModel>(), A.Fake<IModel>(), A.Fake<IModelChanges>()));

        Assert.That(disposable, Is.Not.Null);
        Assert.That(subscriptionWasCalled, Is.True);
    }

    [Test]
    public void PublishChangesWillNotifyModelShardSubscriptionsTest()
    {
        var subscription = new ModelSubscription();

        var subscriptionWasCalled = false;
        var disposable = subscription.GetOrCreateSubscriptionFor<IFakeChangesFrame>().Add(m => subscriptionWasCalled = true);
        var modelChanges = A.Fake<IModelChanges>();
        IFakeChangesFrame? ignore = null;
        var frame = A.Fake<IFakeChangesFrame>();

        A.CallTo(() => frame.HasChanges()).Returns(true);
        A.CallTo(() => modelChanges.TryGetFrame<IFakeChangesFrame>(out ignore))
            .Returns(true)
            .AssignsOutAndRefParameters(frame);

        subscription.Publish(new Change<IModelChanges>(A.Fake<IModel>(), A.Fake<IModel>(), modelChanges));

        Assert.That(disposable, Is.Not.Null);
        Assert.That(subscriptionWasCalled, Is.True);
    }
}
