namespace Navitski.Crystalized.Model.Engine.ChangesTracking;

/// <summary>
///     A writable counterpart of an <see cref="IModelChanges"/>
/// </summary>
/// <remarks>
///     <see cref="IModelChanges"/> is a read-only part of a model changes which
///     is accessible by the developer to analyze changes. On the other hand,
///     <see cref="IWritableModelChanges"/> is a writable part of a model changes
///     which can be used to implement history of changes and undo/redo logic.
///     <see cref="IWritableModelChanges"/> should not be used when handling event
///     of a model change.
/// </remarks>
public interface IWritableModelChanges : IModelChanges
{
    /// <summary>
    ///     Registers an empty changes frame which will hold all changes happened with a model shard
    /// </summary>
    /// <typeparam name="T">A concrete type of a changes frame</typeparam>
    /// <param name="changesFrame">An instance of a empty change frame</param>
    /// <returns>A writable part of a changes frame</returns>
    T Register<T>(T changesFrame) where T : class, IWritableChangesFrame;

    /// <summary>
    ///     Creates a new <see cref="IWritableModelChanges"/> which holds the changes opposite to the original changes
    /// </summary>
    /// <returns>A new inverted changes</returns>
    IWritableModelChanges Invert();

    /// <summary>
    ///     Applies changes to the given model
    /// </summary>
    /// <param name="model">A target model</param>
    void Apply(IModel model);

    /// <summary>
    ///     Merges two <see cref="IWritableModelChanges"/>s into one,
    ///     reducing a number of operations (changes) stored in the <see cref="IWritableModelChanges"/>.
    /// </summary>
    /// <remarks>
    ///     It helps to optimize count of actions needed to be performed to update stored data to the latest version
    /// </remarks>
    /// <param name="changes">Changes, that have happened after the current ones</param>
    /// <returns>Merged changes by combining current changes with the newest</returns>
    IWritableModelChanges Merge(IModelChanges changes);
}
