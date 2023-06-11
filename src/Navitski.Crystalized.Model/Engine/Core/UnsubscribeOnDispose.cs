namespace Navitski.Crystalized.Model.Engine.Core;

internal sealed class UnsubscribeOnDispose<T> : DisposableBase
{
    private readonly HashSet<IObserver<T>> _observers;
    private readonly IObserver<T> _observer;

    public UnsubscribeOnDispose(IObserver<T> observer, HashSet<IObserver<T>> subscriptions)
    {
        _observer = observer;
        _observers = subscriptions;
    }

    protected override void DisposeManagedObjects()
    {
        _observers.Remove(_observer);
    }
}
