using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Persistence;

/// <summary>
///     A storage abstraction for a model shard.
/// </summary>
/// <remarks>
///     A storage is used to store model shards. For each model shard
///     concrete <see cref="IModelShardStorage"/> implementation is created
///     which knows how to write or read data into the model. This is an abstraction
///     over the structure of the concrete model shard, but for real implementation
///     of reading and writing data to the database or files see the implementations
///     of <see cref="IRepository"/> interface. To recap, <see cref="IModelShardStorage"/>,
///     knowing what type of model shard it should read or write, uses a specific implementation
///     of the <see cref="IRepository"/> to perform actual reading or writing.
/// </remarks>
public interface IModelShardStorage
{
    /// <summary>
    ///     Updates data by applying changes to stored data, using <see cref="IRepository"/>
    /// </summary>
    /// <param name="repository">A repository implementation (for example SQLite)</param>
    /// <param name="model">A model after all changes applied</param>
    /// <param name="changes">A collection of model changes which should be saved</param>
    void Update(IRepository repository, IModel model, IModelChanges changes);

    /// <summary>
    ///     Saves all data from the model, using <see cref="IRepository"/>
    /// </summary>
    /// <remarks>
    ///     Use this method to save model data to the empty storage (for example SQLite database)
    /// </remarks>
    /// <param name="repository">A repository implementation (for example SQLite)</param>
    /// <param name="model">A model after all changes applied</param>
    void Save(IRepository repository, IModel model);

    /// <summary>
    ///     Loads data from some storage, using <see cref="IRepository"/>
    /// </summary>
    /// <param name="repository">A repository implementation (for example SQLite)</param>
    /// <param name="model">A model after all changes applied</param>
    void Load(IRepository repository, IModel model);
}
