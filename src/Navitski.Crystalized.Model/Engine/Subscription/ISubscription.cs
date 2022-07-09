namespace Navitski.Crystalized.Model.Engine.Subscription;

internal interface ISubscription<T>
{
    void Push(Message<T> message);
}
