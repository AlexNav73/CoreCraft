using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Subscription;

namespace CoreCraft.Tests.Subscription;

public class UnsubscribeOnDisposeTests
{
    [Test]
    public void UnsubscribeHappensTest()
    {
        var method = A.Fake<IObserver<Change<IModelChanges>>>();
        var subscriptions = new HashSet<IObserver<Change<IModelChanges>>>() { method };
        var subscription = new UnsubscribeOnDispose<IObserver<Change<IModelChanges>>>(method, s => subscriptions.Remove(s));

        Assert.That(subscriptions, Has.Member(method));

        subscription.Dispose();

        Assert.That(subscriptions, Is.Empty);
    }
}
