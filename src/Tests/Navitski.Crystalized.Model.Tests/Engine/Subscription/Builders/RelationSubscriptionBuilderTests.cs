using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Subscription;
using Navitski.Crystalized.Model.Engine.Subscription.Builders;
using Navitski.Crystalized.Model.Engine.Subscription.Extensions;

namespace Navitski.Crystalized.Model.Tests.Engine.Subscription.Builders;

internal class RelationSubscriptionBuilderTests
{
    [Test]
    public void SubscribeWithCurrentChangesTest()
    {
        var changesFrame = A.Fake<IFakeChangesFrame>();
        var relation = A.Fake<IRelationChangeSet<FirstEntity, SecondEntity>>();

        A.CallTo(() => changesFrame.OneToOneRelation).Returns(relation);
        A.CallTo(() => relation.HasChanges()).Returns(true);

        var changes = new Change<IFakeChangesFrame>(A.Fake<IModel>(), A.Fake<IModel>(), changesFrame);
        var subscription = new RelationSubscription<IFakeChangesFrame, FirstEntity, SecondEntity>(x => x.OneToOneRelation);
        var subscriptionBuilder = new RelationSubscriptionBuilder<IFakeChangesFrame, FirstEntity, SecondEntity>(subscription, changes);
        var handlerCalledDuringSubscription = false;

        subscriptionBuilder.Subscribe(x => handlerCalledDuringSubscription = true);

        Assert.That(handlerCalledDuringSubscription, Is.True);
    }

    [Test]
    public void SubscribeWithoutCurrentChangesTest()
    {
        var subscription = new RelationSubscription<IFakeChangesFrame, FirstEntity, SecondEntity>(x => x.OneToOneRelation);
        var subscriptionBuilder = new RelationSubscriptionBuilder<IFakeChangesFrame, FirstEntity, SecondEntity>(subscription, null);
        var handlerCalledDuringSubscription = false;

        subscriptionBuilder.Subscribe(x => handlerCalledDuringSubscription = true);

        Assert.That(handlerCalledDuringSubscription, Is.False);
    }
}
