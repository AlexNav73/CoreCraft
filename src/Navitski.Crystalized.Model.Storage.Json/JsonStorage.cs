using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Persistence;
using Navitski.Crystalized.Model.Storage.Json.Model;

namespace Navitski.Crystalized.Model.Storage.Json;

/// <summary>
///     A Json storage implementation for the domain model
/// </summary>
public sealed class JsonStorage : IStorage
{
    private readonly IJsonFileHandler _jsonFileHandler;
    private readonly IEnumerable<IModelShardStorage> _storages;

    /// <summary>
    ///     Ctor
    /// </summary>
    public JsonStorage(IEnumerable<IModelShardStorage> storages)
        : this(storages, new JsonFileHandler())
    {
    }

    internal JsonStorage(
        IEnumerable<IModelShardStorage> storages,
        IJsonFileHandler jsonFileHandler)
    {
        _storages = storages;
        _jsonFileHandler = jsonFileHandler;
    }

    /// <inheritdoc/>
    public void Update(string path, IModelChanges changes)
    {
        var shards = _jsonFileHandler.ReadModelShardsFromFile(path);
        var repository = new JsonRepository(shards);

        foreach (var storage in _storages)
        {
            storage.Update(repository, changes);
        }

        _jsonFileHandler.WriteModelShardsToFile(path, shards);
    }

    /// <inheritdoc/>
    public void Save(string path, IModel model)
    {
        var shards = new List<ModelShard>();
        var repository = new JsonRepository(shards);

        foreach (var storage in _storages)
        {
            storage.Save(repository, model);
        }

        _jsonFileHandler.WriteModelShardsToFile(path, shards);
    }

    /// <inheritdoc/>
    public void Load(string path, IModel model)
    {
        var shards = _jsonFileHandler.ReadModelShardsFromFile(path);

        var repository = new JsonRepository(shards);

        foreach (var storage in _storages)
        {
            storage.Load(repository, model);
        }
    }
}
