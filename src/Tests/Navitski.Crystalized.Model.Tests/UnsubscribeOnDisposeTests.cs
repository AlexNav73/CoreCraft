using Navitski.Crystalized.Model.Engine.Core;

namespace Navitski.Crystalized.Model.Tests;

public class UnsubscribeOnDisposeTests
{
    [Test]
    public void UnsubscribeHappendsTest()
    {
        var method = (ModelChangedEventArgs args) => { };
        var subscriptions = new HashSet<Action<ModelChangedEventArgs>>() { method };
        var subscription = new UnsubscribeOnDispose(method, subscriptions);

        Assert.That(subscriptions, Has.Member(method));

        subscription.Dispose();

        Assert.That(subscriptions, Is.Empty);
    }
}
