using CoreCraft.Core;
using CoreCraft.Subscription.Builders;

namespace CoreCraft.Storage.Linq2Db.Views;

/// <summary>
///     Represents a view of a relation between parent and child entities.
/// </summary>
/// <remarks>
///     This interface simplifies working with relations by removing the
///     need to retrieve the model shard containing the relation each time the domain
///     model changes. Views provide easy access to the most up-to-date entities
///     and properties, and also make it straightforward to subscribe to relation changes.
///     They are aware of the types of changes they observe and have access to the
///     domain model's subscription mechanism.
/// </remarks>
/// <typeparam name="TParent">The parent entity type.</typeparam>
/// <typeparam name="TChild">The child entity type.</typeparam>
public interface ILazyRelationView<TParent, TChild> :
    ILazyRelation<TParent, TChild>,
    IRelationSubscriptionBuilder<TParent, TChild>,
    IDisposable
    where TParent : Entity
    where TChild : Entity
{
}
