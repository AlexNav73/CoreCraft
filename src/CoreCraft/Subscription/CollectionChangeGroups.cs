using System.Diagnostics.CodeAnalysis;
using CoreCraft.ChangesTracking;

namespace CoreCraft.Subscription;

/// <summary>
///     Changes happened with binded collection
/// </summary>
/// <typeparam name="TEntity">An entity type</typeparam>
/// <typeparam name="TProperties">A type of a properties</typeparam>
[ExcludeFromCodeCoverage]
public sealed class CollectionChangeGroups<TEntity, TProperties>
    where TEntity : Entity
    where TProperties : Properties
{
    internal CollectionChangeGroups(
        IEnumerable<IEntityChange<TEntity, TProperties>> added,
        IEnumerable<IEntityChange<TEntity, TProperties>> removed,
        IEnumerable<IEntityChange<TEntity, TProperties>> modified)
    {
        Added = added;
        Removed = removed;
        Modified = modified;
    }

    /// <summary>
    ///     A collection of added entities
    /// </summary>
    public IEnumerable<IEntityChange<TEntity, TProperties>> Added { get; }

    /// <summary>
    ///     A collection of entities which were removed
    /// </summary>
    public IEnumerable<IEntityChange<TEntity, TProperties>> Removed { get; }

    /// <summary>
    ///     A collection of changed entities
    /// </summary>
    public IEnumerable<IEntityChange<TEntity, TProperties>> Modified { get; }
}
