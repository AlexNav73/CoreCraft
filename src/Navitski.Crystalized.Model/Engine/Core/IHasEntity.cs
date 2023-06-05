namespace Navitski.Crystalized.Model.Engine.Core;

/// <summary>
///     Represents an interface for accessing an entity object of type <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TEntity">The type of the entity object.</typeparam>
public interface IHasEntity<out TEntity>
    where TEntity : Entity
{
    /// <summary>
    ///     Gets the entity object.
    /// </summary>
    TEntity Entity { get; }
}
