using CoreCraft.ChangesTracking;

namespace CoreCraft.Core;

/// <summary>
///     Provides a factory for creating instances of changes frames specific to model shard types.
/// </summary>
public interface IFrameFactory
{
    /// <summary>
    ///     Creates a new instance of a changes frame or returns existing one suitable for the model shard type this factory is associated with.
    /// </summary>
    /// <returns>
    ///     An instance of the <see cref="IChangesFrame" /> interface representing a changes frame for the specific model shard type.
    /// </returns>
    /// <remarks>
    /// This interface allows for creating changes frames that are tailored to the specific data structures and change tracking requirements of different model shard types.
    /// - The factory pattern decouples the creation of changes frames from their usage, promoting flexibility and maintainability.
    /// - By calling <see cref="Create" />, you obtain a new instance of <see cref="IChangesFrame" /> that can be used to track and manage changes within a specific model shard.
    /// </remarks>
    IChangesFrame Create();
}
