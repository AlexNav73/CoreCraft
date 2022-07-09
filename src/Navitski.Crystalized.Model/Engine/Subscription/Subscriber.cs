using Navitski.Crystalized.Model.Engine.Exceptions;

namespace Navitski.Crystalized.Model.Engine.Subscription;

internal abstract class Subscriber<T>
{
    private readonly HashSet<Action<Message<T>>> _subscriptions;

    public Subscriber()
    {
        _subscriptions = new HashSet<Action<Message<T>>>();
    }

    public IDisposable Subscribe(Action<Message<T>> onModelChanges)
    {
        if (_subscriptions.Contains(onModelChanges))
        {
            throw new SubscriptionAlreadyExistsException("Subscription already exists");
        }

        _subscriptions.Add(onModelChanges);

        return new UnsubscribeOnDispose<Message<T>>(onModelChanges, _subscriptions);
    }

    protected void Notify(Message<T> message)
    {
        foreach (var subscription in _subscriptions)
        {
            subscription(message);
        }
    }
}
