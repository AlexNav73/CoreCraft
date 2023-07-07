namespace CoreCraft.Subscription;

internal sealed class AnonymousObserver<T> : IObserver<T>
{
    private readonly Action<T> _action;

    public AnonymousObserver(Action<T> action)
    {
        _action = action;
    }

    public void OnNext(T value)
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
