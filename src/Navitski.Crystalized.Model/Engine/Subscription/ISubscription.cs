namespace Navitski.Crystalized.Model.Engine.Subscription;

internal interface ISubscription<T>
{
    void Publish(Change<T> change);
}
