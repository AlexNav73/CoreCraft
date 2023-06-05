namespace Navitski.Crystalized.Model.Engine.Subscription.Binding;

/// <summary>
///     Represents an interface for binding a collection of entity objects of type <typeparamref name="TEntity"/>
///     to a collection change handler for handling changes.
/// </summary>
/// <typeparam name="TEntity">The type of the entity object.</typeparam>
/// <typeparam name="TProperties">The type of the properties associated with the entity.</typeparam>
public interface ICollectionBinding<TEntity, TProperties>
    where TEntity : Entity
    where TProperties : Properties
{
    /// <summary>
    ///     Called when the collection of entities is changed.
    /// </summary>
    /// <param name="changes">The changes that occurred in the collection.</param>
    void OnCollectionChanged(BindingChanges<TEntity, TProperties> changes);
}
