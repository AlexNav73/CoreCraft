namespace Navitski.Crystalized.Model.Engine.ChangesTracking;

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
}
