namespace CoreCraft.Engine.Subscription.Builders;

/// <summary>
///     A common interface for all subscription builders
/// </summary>
/// <typeparam name="T">A change type</typeparam>
public interface ISubscriptionBuilder<T> : IObservable<Change<T>>
{
}
