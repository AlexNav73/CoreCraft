namespace CoreCraft.ChangesTracking;

/// <summary>
///     An entity change
/// </summary>
/// <typeparam name="TEntity">An entity type</typeparam>
/// <typeparam name="TProperties">A type of a properties</typeparam>
public interface IEntityChange<out TEntity, TProperties> : IHasEntity<TEntity>
    where TEntity : Entity
    where TProperties : Properties
{
    /// <summary>
    ///     An old properties of an entity
    /// </summary>
    TProperties? OldData { get; }

    /// <summary>
    ///     A new properties of an entity
    /// </summary>
    TProperties? NewData { get; }
}
