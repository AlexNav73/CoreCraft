using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Subscription;
using Navitski.Crystalized.Model.Engine.Subscription.Builders;

namespace Navitski.Crystalized.Model.Tests;

internal class CollectionSubscriptionBuilderTests
{
    [Test]
    public void SubscribeWithCurrentChangesTest()
    {
        var changesFrame = A.Fake<IFakeChangesFrame>();
        var collection = A.Fake<ICollectionChangeSet<FirstEntity, FirstEntityProperties>>();

        A.CallTo(() => changesFrame.FirstCollection).Returns(collection);
        A.CallTo(() => collection.HasChanges()).Returns(true);

        var changes = new Change<IFakeChangesFrame>(A.Fake<IModel>(), A.Fake<IModel>(), changesFrame);
        var subscription = new CollectionSubscription<IFakeChangesFrame, FirstEntity, FirstEntityProperties>(x => x.FirstCollection);
        var subscriptionBuilder = new CollectionSubscriptionBuilder<IFakeChangesFrame, FirstEntity, FirstEntityProperties>(subscription, changes);
        var handlerCalledDuringSubscription = false;

        subscriptionBuilder.Subscribe(x => handlerCalledDuringSubscription = true);

        Assert.That(handlerCalledDuringSubscription, Is.True);
    }

    [Test]
    public void SubscribeWithoutCurrentChangesTest()
    {
        var subscription = new CollectionSubscription<IFakeChangesFrame, FirstEntity, FirstEntityProperties>(x => x.FirstCollection);
        var subscriptionBuilder = new CollectionSubscriptionBuilder<IFakeChangesFrame, FirstEntity, FirstEntityProperties>(subscription, null);
        var handlerCalledDuringSubscription = false;

        subscriptionBuilder.Subscribe(x => handlerCalledDuringSubscription = true);

        Assert.That(handlerCalledDuringSubscription, Is.False);
    }
}
