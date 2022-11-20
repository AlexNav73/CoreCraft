namespace Navitski.Crystalized.Model.Engine.ChangesTracking;

/// <summary>
///     A mutable counterpart of a <see cref="IChangesFrame"/>
/// </summary>
/// <remarks>
///     <see cref="IChangesFrame"/> is a read-only part of a changes frame which
///     is accessible by the developer to analyze changes. On the other hand,
///     <see cref="IWritableChangesFrame"/> is a writable part of a changes frame
///     which can be used to implement history of changes and undo/redo logic.
///     <see cref="IWritableChangesFrame"/> should not be used when handling event
///     of a model change.
/// </remarks>
public interface IWritableChangesFrame : IChangesFrame
{
    /// <summary>
    ///     Applies changes to the given model
    /// </summary>
    /// <param name="model">A target model</param>
    void Apply(IModel model);

    /// <summary>
    ///     Creates a new <see cref="IWritableChangesFrame"/> which holds the changes opposite to the original changes
    /// </summary>
    /// <returns>A new inverted changes</returns>
    IWritableChangesFrame Invert();

    /// <summary>
    ///     Merges two <see cref="IWritableChangesFrame"/>s into one,
    ///     reducing a number of operations (changes) stored in the <see cref="IWritableChangesFrame"/>.
    /// </summary>
    /// <remarks>
    ///     It helps to optimize count of actions needed to be performed to update stored data to the latest version
    /// </remarks>
    /// <param name="frame">Changes, that have happened after the current ones</param>
    /// <returns>Merged frames by combining current frame with the newest</returns>
    IWritableChangesFrame Merge(IChangesFrame frame);
}
