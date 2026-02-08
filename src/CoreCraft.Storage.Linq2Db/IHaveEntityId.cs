using CoreCraft.Core;

namespace CoreCraft.Storage.Linq2Db;

/// <summary>
///     Represents an object that has an entity identifier.
/// </summary>
public interface IHaveEntityId<out TEntity> where TEntity : Entity
{
    /// <summary>
    ///     Gets the unique identifier of the entity.
    /// </summary>
    TEntity EntityId { get; }
}
