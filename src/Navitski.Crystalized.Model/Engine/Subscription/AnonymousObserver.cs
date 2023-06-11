namespace Navitski.Crystalized.Model.Engine.Subscription;

internal sealed class AnonymousObserver<T> : IObserver<Change<T>>
{
    private readonly Action<Change<T>> _action;

    public AnonymousObserver(Action<Change<T>> action)
    {
        _action = action;
    }

    public void OnNext(Change<T> value)
    {
        _action(value);
    }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    public override bool Equals(object? obj)
    {
        return _action.Equals(((AnonymousObserver<T>?)obj)?._action);
    }

    public override int GetHashCode()
    {
        return _action.GetHashCode();
    }
}
