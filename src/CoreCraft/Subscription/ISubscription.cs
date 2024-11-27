namespace CoreCraft.Subscription;

internal interface ISubscription<T>
{
    void Publish(Change<T> change, bool forView = false);
}
