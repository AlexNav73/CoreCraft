using CoreCraft.Engine.ChangesTracking;

namespace CoreCraft.Engine.Core;

/// <summary>
///     A context used by features. Provides utility methods for each feature
/// </summary>
public interface IFeatureContext
{
    /// <summary>
    ///     Registers a <see cref="IChangesFrame"/> inside <see cref="IMutableModelChanges"/>
    /// </summary>
    /// <param name="modelChanges">A model changes container</param>
    /// <returns>An instance of specific changes frame</returns>
    IChangesFrame GetOrAddFrame(IMutableModelChanges modelChanges);
}
