using Navitski.Crystalized.Model.Engine.Exceptions;

namespace Navitski.Crystalized.Model.Engine.Subscription;

internal abstract class Subscriber<T>
{
    private readonly HashSet<Action<Message<T>>> _handlers;

    public Subscriber()
    {
        _handlers = new HashSet<Action<Message<T>>>();
    }

    public IDisposable Subscribe(Action<Message<T>> onModelChanges)
    {
        if (_handlers.Contains(onModelChanges))
        {
            throw new SubscriptionAlreadyExistsException("Subscription already exists");
        }

        _handlers.Add(onModelChanges);

        return new UnsubscribeOnDispose<Message<T>>(onModelChanges, _handlers);
    }

    protected void Notify(Message<T> message)
    {
        foreach (var handler in _handlers)
        {
            handler(message);
        }
    }
}
