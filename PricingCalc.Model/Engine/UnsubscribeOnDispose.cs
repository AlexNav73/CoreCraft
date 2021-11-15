namespace PricingCalc.Model.Engine;

internal class UnsubscribeOnDispose : DisposableBase
{
    private readonly HashSet<Action<ModelChangedEventArgs>> _subscriptions;
    private readonly Action<ModelChangedEventArgs> _onModelChanges;

    public UnsubscribeOnDispose(Action<ModelChangedEventArgs> onModelChanges, HashSet<Action<ModelChangedEventArgs>> subscriptions)
    {
        _onModelChanges = onModelChanges;
        _subscriptions = subscriptions;
    }

    protected override void DisposeManagedObjects()
    {
        _subscriptions.Remove(_onModelChanges);
    }
}
