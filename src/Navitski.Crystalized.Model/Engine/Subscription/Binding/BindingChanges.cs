using System.Diagnostics.CodeAnalysis;
using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Subscription.Binding;

/// <summary>
///     Changes happened with binded collection
/// </summary>
/// <typeparam name="TEntity">An entity type</typeparam>
/// <typeparam name="TProperties">A type of a properties</typeparam>
[ExcludeFromCodeCoverage]
public sealed class BindingChanges<TEntity, TProperties>
    where TEntity : Entity
    where TProperties : Properties
{
    internal BindingChanges(
        IModel oldModel,
        IModel newModel,
        IEnumerable<ICollectionChange<TEntity, TProperties>> added,
        IEnumerable<ICollectionChange<TEntity, TProperties>> removed,
        IEnumerable<ICollectionChange<TEntity, TProperties>> modified)
    {
        OldModel = oldModel;
        NewModel = newModel;
        Added = added;
        Removed = removed;
        Modified = modified;
    }

    /// <summary>
    ///     The old version of the domain model
    /// </summary>
    public IModel OldModel { get; }

    /// <summary>
    ///     The new version of the domain model
    /// </summary>
    public IModel NewModel { get; }

    /// <summary>
    ///     A collection of added entities
    /// </summary>
    public IEnumerable<ICollectionChange<TEntity, TProperties>> Added { get; }

    /// <summary>
    ///     A collection of entities which were removed
    /// </summary>
    public IEnumerable<ICollectionChange<TEntity, TProperties>> Removed { get; }

    /// <summary>
    ///     A collection of changed entities
    /// </summary>
    public IEnumerable<ICollectionChange<TEntity, TProperties>> Modified { get; }
}
