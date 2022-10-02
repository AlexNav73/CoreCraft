namespace Navitski.Crystalized.Model.Engine.Core;

internal sealed class UnsubscribeOnDispose<T> : DisposableBase
{
    private readonly HashSet<Action<T>> _subscriptions;
    private readonly Action<T> _onModelChanges;

    public UnsubscribeOnDispose(Action<T> onModelChanges, HashSet<Action<T>> subscriptions)
    {
        _onModelChanges = onModelChanges;
        _subscriptions = subscriptions;
    }

    protected override void DisposeManagedObjects()
    {
        _subscriptions.Remove(_onModelChanges);
    }
}
