namespace Navitski.Crystalized.Model.Engine.Subscription.Binding;

/// <summary>
///     Represents an interface for binding an entity object of type <typeparamref name="TEntity"/>
///     to its corresponding properties of type <typeparamref name="TProperties"/>.
/// </summary>
/// <typeparam name="TEntity">The type of the entity object.</typeparam>
/// <typeparam name="TProperties">The type of the properties associated with the entity.</typeparam>
public interface IEntityBinding<TEntity, TProperties>
    where TEntity : Entity
    where TProperties : Properties
{
    /// <summary>
    ///     Called when the entity's properties are changed.
    /// </summary>
    /// <param name="oldProperties">The previous properties of the entity.</param>
    /// <param name="newProperties">The new properties of the entity.</param>
    void OnEntityChanged(TProperties oldProperties, TProperties newProperties);
}
