using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Subscription;

namespace Navitski.Crystalized.Model.Tests;

internal class ModelSubscriptionTests
{
    [Test]
    public void FirstCallOfToWillCreateNewSubscriberTest()
    {
        var subscriber = new ModelSubscription();

        var shardSubscriber = subscriber.GetOrCreateSubscriberFor<IFakeChangesFrame>();

        Assert.That(shardSubscriber, Is.Not.Null);
    }

    [Test]
    public void SecondCallOfToWillReturnTheSameSubscriberTest()
    {
        var subscriber = new ModelSubscription();

        var shardSubscriber1 = subscriber.GetOrCreateSubscriberFor<IFakeChangesFrame>();
        var shardSubscriber2 = subscriber.GetOrCreateSubscriberFor<IFakeChangesFrame>();

        Assert.That(shardSubscriber1, Is.Not.Null);
        Assert.That(shardSubscriber2, Is.Not.Null);
        Assert.That(ReferenceEquals(shardSubscriber1, shardSubscriber2), Is.True);
    }

    [Test]
    public void PublishChangesWillNotifySubscriptionsTest()
    {
        var subscriber = new ModelSubscription();

        var subscriptionWasCalled = false;
        var subscription = subscriber.Add(m => subscriptionWasCalled = true);

        subscriber.Publish(new Change<IModelChanges>(A.Fake<IModel>(), A.Fake<IModel>(), A.Fake<IModelChanges>()));

        Assert.That(subscription, Is.Not.Null);
        Assert.That(subscriptionWasCalled, Is.True);
    }

    [Test]
    public void PublishChangesWillNotifyModelShardSubscriptionsTest()
    {
        var subscriber = new ModelSubscription();

        var subscriptionWasCalled = false;
        var subscription = subscriber.GetOrCreateSubscriberFor<IFakeChangesFrame>().Add(m => subscriptionWasCalled = true);
        var modelChanges = A.Fake<IModelChanges>();
        IFakeChangesFrame? ignore = null;
        var frame = A.Fake<IFakeChangesFrame>();

        A.CallTo(() => frame.HasChanges()).Returns(true);
        A.CallTo(() => modelChanges.TryGetFrame<IFakeChangesFrame>(out ignore))
            .Returns(true)
            .AssignsOutAndRefParameters(frame);

        subscriber.Publish(new Change<IModelChanges>(A.Fake<IModel>(), A.Fake<IModel>(), modelChanges));

        Assert.That(subscription, Is.Not.Null);
        Assert.That(subscriptionWasCalled, Is.True);
    }
}
