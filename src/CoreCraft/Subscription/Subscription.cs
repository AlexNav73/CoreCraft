using CoreCraft.Exceptions;

namespace CoreCraft.Subscription;

internal abstract class Subscription<T> : ISubscription<T>, IObservable<Change<T>>
{
    private readonly HashSet<IObserver<Change<T>>> _handlers;

    protected Subscription()
    {
        _handlers = new HashSet<IObserver<Change<T>>>();
    }

    public IDisposable Subscribe(IObserver<Change<T>> onModelChanges)
    {
        if (_handlers.Contains(onModelChanges))
        {
            throw new SubscriptionAlreadyExistsException("Subscription already exists");
        }

        _handlers.Add(onModelChanges);

        return new UnsubscribeOnDispose<IObserver<Change<T>>>(onModelChanges, s => _handlers.Remove(s));
    }

    public virtual void Publish(Change<T> change, bool forView)
    {
        if (forView)
        {
            return;
        }

        foreach (var handler in _handlers.ToArray())
        {
            handler.OnNext(change);
        }
    }
}
