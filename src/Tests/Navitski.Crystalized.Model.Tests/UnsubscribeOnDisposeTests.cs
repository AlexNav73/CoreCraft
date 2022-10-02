using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Subscription;

namespace Navitski.Crystalized.Model.Tests;

public class UnsubscribeOnDisposeTests
{
    [Test]
    public void UnsubscribeHappendsTest()
    {
        var method = (Change<IModelChanges> args) => { };
        var subscriptions = new HashSet<Action<Change<IModelChanges>>>() { method };
        var subscription = new UnsubscribeOnDispose<Change<IModelChanges>>(method, subscriptions);

        Assert.That(subscriptions, Has.Member(method));

        subscription.Dispose();

        Assert.That(subscriptions, Is.Empty);
    }
}
