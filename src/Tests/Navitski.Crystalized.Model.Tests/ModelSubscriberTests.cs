using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Subscription;

namespace Navitski.Crystalized.Model.Tests;

internal class ModelSubscriberTests
{
    [Test]
    public void FirstCallOfToWillCreateNewSubscriberTest()
    {
        var subscriber = new ModelSubscriber();

        var shardSubscriber = subscriber.To<IFakeChangesFrame>();

        Assert.That(shardSubscriber, Is.Not.Null);
    }

    [Test]
    public void SecondCallOfToWillReturnTheSameSubscriberTest()
    {
        var subscriber = new ModelSubscriber();

        var shardSubscriber1 = subscriber.To<IFakeChangesFrame>();
        var shardSubscriber2 = subscriber.To<IFakeChangesFrame>();

        Assert.That(shardSubscriber1, Is.Not.Null);
        Assert.That(shardSubscriber2, Is.Not.Null);
        Assert.That(ReferenceEquals(shardSubscriber1, shardSubscriber2), Is.True);
    }

    [Test]
    public void PushChangesWillNotifySubscriptionsTest()
    {
        var subscriber = new ModelSubscriber();

        var subscriptionWasCalled = false;
        var subscription = subscriber.Subscribe(m => subscriptionWasCalled = true);

        subscriber.Push(new Message<IModelChanges>(A.Fake<IModel>(), A.Fake<IModel>(), A.Fake<IModelChanges>()));

        Assert.That(subscription, Is.Not.Null);
        Assert.That(subscriptionWasCalled, Is.True);
    }

    [Test]
    public void PushChangesWillNotifyModelShardSubscriptionsTest()
    {
        var subscriber = new ModelSubscriber();

        var subscriptionWasCalled = false;
        var subscription = subscriber.To<IFakeChangesFrame>().Subscribe(m => subscriptionWasCalled = true);
        var modelChanges = A.Fake<IModelChanges>();
        IFakeChangesFrame? ignore = null;
        var frame = A.Fake<IFakeChangesFrame>();

        A.CallTo(() => frame.HasChanges()).Returns(true);
        A.CallTo(() => modelChanges.TryGetFrame<IFakeChangesFrame>(out ignore))
            .Returns(true)
            .AssignsOutAndRefParameters(frame);

        subscriber.Push(new Message<IModelChanges>(A.Fake<IModel>(), A.Fake<IModel>(), modelChanges));

        Assert.That(subscription, Is.Not.Null);
        Assert.That(subscriptionWasCalled, Is.True);
    }
}
