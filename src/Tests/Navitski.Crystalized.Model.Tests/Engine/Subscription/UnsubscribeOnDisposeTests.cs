using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Subscription;

namespace Navitski.Crystalized.Model.Tests.Engine.Subscription;

public class UnsubscribeOnDisposeTests
{
    [Test]
    public void UnsubscribeHappensTest()
    {
        var method = A.Fake<IObserver<Change<IModelChanges>>>();
        var subscriptions = new HashSet<IObserver<Change<IModelChanges>>>() { method };
        var subscription = new UnsubscribeOnDispose<Change<IModelChanges>>(method, subscriptions);

        Assert.That(subscriptions, Has.Member(method));

        subscription.Dispose();

        Assert.That(subscriptions, Is.Empty);
    }
}
