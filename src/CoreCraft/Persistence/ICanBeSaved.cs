using CoreCraft.ChangesTracking;

namespace CoreCraft.Persistence;

/// <summary>
///     Represents an interface for objects that can be saved using a specified repository.
/// </summary>
/// <remarks>
///     This interface can be implemented for both <see cref="IModelShard"/>s and <see cref="IChangesFrame" />s
/// </remarks>
public interface ICanBeSaved
{
    /// <summary>
    ///     Saves the implementing object using the provided repository.
    /// </summary>
    /// <param name="repository">The repository used to save the object.</param>
    void Save(IRepository repository);
}
