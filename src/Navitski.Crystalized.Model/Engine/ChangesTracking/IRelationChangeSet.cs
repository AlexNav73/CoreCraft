namespace Navitski.Crystalized.Model.Engine.ChangesTracking;

/// <summary>
///     A container of changes performed on the relation
/// </summary>
/// <typeparam name="TParent">A type of a parent entity</typeparam>
/// <typeparam name="TChild">A type of a child entity</typeparam>
/// <remarks>
///     When a command executes, all changes to the model are recorded in change sets.
///     All change sets are combined in a single <see cref="IChangesFrame"/> which can
///     be queried for a specific change by an entity. Changes can be inverted to produce
///     "undo" changes which can revert relation to the state, before modification.
///     When needed, change sets can be applied to the collection or relation to migrate it
///     to the newer version
/// </remarks>
public interface IRelationChangeSet<TParent, TChild> : IEnumerable<IRelationChange<TParent, TChild>>
    where TParent : Entity
    where TChild : Entity
{
    /// <summary>
    ///     Adds new change record to the change set
    /// </summary>
    /// <param name="action">An action performed</param>
    /// <param name="parent">A parent entity</param>
    /// <param name="child">A child entity</param>
    void Add(RelationAction action, TParent parent, TChild child);

    /// <summary>
    ///     Creates a new <see cref="IRelationChangeSet{TParent, TChild}"/> which holds the changes opposite to the original changes
    /// </summary>
    /// <returns>A new inverted changes</returns>
    /// <exception cref="NotSupportedException">Throws when at least one change has an Action with a wrong value</exception>
    IRelationChangeSet<TParent, TChild> Invert();

    /// <summary>
    ///     Applies changes to the given relation
    /// </summary>
    /// <param name="relation">A target relation</param>
    /// <exception cref="NotSupportedException">Throws when at least one change has an Action with a wrong value</exception>
    void Apply(IMutableRelation<TParent, TChild> relation);

    /// <summary>
    ///     If a change set is not empty
    /// </summary>
    /// <returns>True - if a change set holds some changes</returns>
    bool HasChanges();
}
