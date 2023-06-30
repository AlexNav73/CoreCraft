namespace CoreCraft.Engine.ChangesTracking;

/// <summary>
///     A specific change, recorded when a relation was modified
/// </summary>
/// <typeparam name="TParent">A parent entity type</typeparam>
/// <typeparam name="TChild">A child entity type</typeparam>
public interface IRelationChange<TParent, TChild>
    where TParent : Entity
    where TChild : Entity
{
    /// <summary>
    ///     Action which was performed on the relation
    /// </summary>
    RelationAction Action { get; }

    /// <summary>
    ///     A parent entity
    /// </summary>
    TParent Parent { get; }

    /// <summary>
    ///     A child entity
    /// </summary>
    TChild Child { get; }

    /// <summary>
    ///     Inverts a change
    /// </summary>
    /// <returns>A new change which is opposite to the original one</returns>
    /// <exception cref="NotSupportedException">Throws when an Action has wrong value</exception>
    IRelationChange<TParent, TChild> Invert();
}
