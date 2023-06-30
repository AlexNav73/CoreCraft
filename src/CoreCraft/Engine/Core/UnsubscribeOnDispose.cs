namespace CoreCraft.Engine.Core;

internal sealed class UnsubscribeOnDispose<T> : DisposableBase
{
    private readonly Action<T> _unsubscribeAction;
    private readonly T _observer;

    public UnsubscribeOnDispose(T observer, Action<T> unsubscribeAction)
    {
        _observer = observer;
        _unsubscribeAction = unsubscribeAction;
    }

    protected override void DisposeManagedObjects()
    {
        _unsubscribeAction(_observer);
    }
}
