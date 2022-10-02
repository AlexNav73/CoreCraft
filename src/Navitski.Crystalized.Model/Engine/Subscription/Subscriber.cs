using Navitski.Crystalized.Model.Engine.Exceptions;

namespace Navitski.Crystalized.Model.Engine.Subscription;

internal abstract class Subscriber<T> : ISubscriber<T>, ISubscription<T>
{
    private readonly HashSet<Action<Change<T>>> _handlers;

    public Subscriber()
    {
        _handlers = new HashSet<Action<Change<T>>>();
    }

    public IDisposable By(Action<Change<T>> onModelChanges)
    {
        if (_handlers.Contains(onModelChanges))
        {
            throw new SubscriptionAlreadyExistsException("Subscription already exists");
        }

        _handlers.Add(onModelChanges);

        return new UnsubscribeOnDispose<Change<T>>(onModelChanges, _handlers);
    }

    public virtual void Publish(Change<T> change)
    {
        foreach (var handler in _handlers)
        {
            handler(change);
        }
    }
}
