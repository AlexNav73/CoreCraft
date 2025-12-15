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
    /// <param name="token"></param>
    Task ApplyAsync(IModel model, CancellationToken token = default);

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
    ///     Creates a new <see cref="IChangesFrame"/> which holds the changes opposite to the original changes
    /// </summary>
    /// <returns>A new inverted changes</returns>
    IChangesFrame Invert();

    /// <summary>
    ///     Executes a operation on the changes represented by this frame.
    /// </summary>
    /// <typeparam name="T">The type of the class implementing <see cref="IChangesFrameOperation"/>.</typeparam>
    /// <param name="operation">
    ///     This object will perform specific operations on the changes within the frame.
    /// </param>
    /// <remarks>
    ///     This method allows applying logic to the changes stored in the current <see cref="IChangesFrameEx"/> object.
    ///     This object can then access and process the collection and relation changes through
    ///     the <see cref="IChangesFrameOperation.OnCollection"/> and <see cref="IChangesFrameOperation.OnRelation"/> methods.
    ///     This approach allows for flexible and decoupled processing of changes.
    /// </remarks>
    void Do<T>(T operation) where T : IChangesFrameOperation;
}
