using CoreCraft.Persistence;
using CoreCraft.Persistence.History;

namespace CoreCraft.ChangesTracking;

/// <summary>
///     An extension of the <see cref="IChangesFrame"/> which used internally in the <see cref="DomainModel"/>
/// </summary>
public interface IChangesFrameEx : IChangesFrame
{
    /// <summary>
    ///     Applies changes to the given model
    /// </summary>
    /// <param name="model">A target model</param>
    void Apply(IModel model);

    /// <summary>
    ///     Merges two <see cref="IChangesFrameEx"/>s into one,
    ///     reducing a number of operations (changes) stored in the <see cref="IChangesFrameEx"/>.
    /// </summary>
    /// <remarks>
    ///     It helps to optimize count of actions needed to be performed to update stored data to the latest version
    /// </remarks>
    /// <param name="frame">Changes, that have happened after the current ones</param>
    /// <returns>Merged frames by combining current frame with the newest</returns>
    IChangesFrame Merge(IChangesFrame frame);

    /// <summary>
    ///     Retrieves a collection's changes set
    /// </summary>
    /// <typeparam name="TEntity">A type of a collection's entity</typeparam>
    /// <typeparam name="TProperty">A type of a collection's properties</typeparam>
    /// <param name="collection">A collection to query a change set</param>
    /// <returns>A collection's change set</returns>
    ICollectionChangeSet<TEntity, TProperty>? Get<TEntity, TProperty>(ICollection<TEntity, TProperty> collection)
        where TEntity : Entity
        where TProperty : Properties;

    /// <summary>
    ///     Retrieves a relation's changes set
    /// </summary>
    /// <typeparam name="TParent">A type of a relation's parent entity</typeparam>
    /// <typeparam name="TChild">A type of a relation's child entity</typeparam>
    /// <param name="relation">A relation to query a change set</param>
    /// <returns>A relation's change set</returns>
    IRelationChangeSet<TParent, TChild>? Get<TParent, TChild>(IRelation<TParent, TChild> relation)
        where TParent : Entity
        where TChild : Entity;

    /// <summary>
    ///     Creates a new <see cref="IChangesFrame"/> which holds the changes opposite to the original changes
    /// </summary>
    /// <returns>A new inverted changes</returns>
    IChangesFrame Invert();

    /// <summary>
    ///     Saves the changes frame using the provided repository.
    /// </summary>
    /// <param name="repository">The repository used to save the changes frame.</param>
    void Update(IRepository repository);

    /// <summary>
    ///     Saves the changes stored in this frame to the specified repository.
    /// </summary>
    /// <param name="changeId">A unique identifier for the change set.</param>
    /// <param name="repository">The <see cref="IHistoryRepository" /> instance used to persist the changes.</param>
    /// <remarks>
    ///     This method persists the changes tracked within this <see cref="IChangesFrame"/> instance to the history storage using the provided `repository`.
    ///     - The `changeId` parameter allows for associating the changes with a specific event or action.
    ///     - The `repository` parameter is an <see cref="IHistoryRepository" /> instance responsible for handling the storage and retrieval of history data.
    /// </remarks>
    void Save(int changeId, IHistoryRepository repository);

    /// <summary>
    ///     Loads changes history from the specified repository for the given change identifier.
    /// </summary>
    /// <param name="changeId">A unique identifier for the change set to load.</param>
    /// <param name="repository">The <see cref="IHistoryRepository" /> instance used to retrieve the changes.</param>
    /// <remarks>
    ///     This method retrieves changes associated with the provided `changeId` from the storage using the specified `repository`.
    ///     - The `changeId` parameter specifies the unique identifier of the change set to be loaded.
    ///     - The `repository` parameter is an <see cref="IHistoryRepository" /> instance responsible for providing access to changes.
    /// </remarks>
    void Load(int changeId, IHistoryRepository repository);
}
