using CoreCraft.ChangesTracking;
using CoreCraft.Subscription;

namespace CoreCraft.Tests.Subscription;

public class AnonymousObserverTests
{
    [Test]
    public void GetHashCodeOfObserverIsTheSameAsOfHandlerTest()
    {
        Action<Change<ICollectionChange<FirstEntity, FirstEntityProperties>>> handler = changes => { };
        var observer = new AnonymousObserver<Change<ICollectionChange<FirstEntity, FirstEntityProperties>>>(handler);

        Assert.That(observer.GetHashCode(), Is.EqualTo(handler.GetHashCode()));
    }

    [Test]
    public void EqualsOverloadComparesHandlersAndNotObserversThemSelfTest()
    {
        Action<Change<ICollectionChange<FirstEntity, FirstEntityProperties>>> handler = changes => { };
        var observer = new AnonymousObserver<Change<ICollectionChange<FirstEntity, FirstEntityProperties>>>(handler);
        var anotherObserver = new AnonymousObserver<Change<ICollectionChange<FirstEntity, FirstEntityProperties>>>(handler);

        Assert.That(observer.Equals(anotherObserver), Is.True);
    }

    [Test]
    public void EqualsReturnsFalseIfComparedWithNullTest()
    {
        Action<Change<ICollectionChange<FirstEntity, FirstEntityProperties>>> handler = changes => { };
        var observer = new AnonymousObserver<Change<ICollectionChange<FirstEntity, FirstEntityProperties>>>(handler);

        Assert.That(observer.Equals(null), Is.False);
    }

    [Test]
    public void OnCompletedDoesNotThrowTest()
    {
        Action<Change<ICollectionChange<FirstEntity, FirstEntityProperties>>> handler = changes => { };
        var observer = new AnonymousObserver<Change<ICollectionChange<FirstEntity, FirstEntityProperties>>>(handler);

        Assert.DoesNotThrow(observer.OnCompleted);
    }

    [Test]
    public void OnErrorDoesNotThrowTest()
    {
        Action<Change<ICollectionChange<FirstEntity, FirstEntityProperties>>> handler = changes => { };
        var observer = new AnonymousObserver<Change<ICollectionChange<FirstEntity, FirstEntityProperties>>>(handler);

        Assert.DoesNotThrow(() => observer.OnError(new Exception()));
    }
}
