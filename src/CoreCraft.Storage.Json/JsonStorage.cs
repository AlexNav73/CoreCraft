using CoreCraft.Engine.ChangesTracking;
using CoreCraft.Engine.Core;
using CoreCraft.Engine.Persistence;
using CoreCraft.Storage.Json.Model;
using Newtonsoft.Json;

namespace CoreCraft.Storage.Json;

/// <summary>
///     A Json storage implementation for the domain model
/// </summary>
public sealed class JsonStorage : IStorage
{
    private readonly JsonSerializerSettings? _settings;
    private readonly IJsonFileHandler _jsonFileHandler;
    private readonly IEnumerable<IModelShardStorage> _storages;

    /// <summary>
    ///     Ctor
    /// </summary>
    public JsonStorage(
        IEnumerable<IModelShardStorage> storages,
        JsonSerializerSettings? options = null)
        : this(storages, new JsonFileHandler(), options)
    {
    }

    internal JsonStorage(
        IEnumerable<IModelShardStorage> storages,
        IJsonFileHandler jsonFileHandler,
        JsonSerializerSettings? options = null)
    {
        _storages = storages;
        _jsonFileHandler = jsonFileHandler;
        _settings = options;
    }

    /// <inheritdoc/>
    public void Update(string path, IModelChanges changes)
    {
        var shards = _jsonFileHandler.ReadModelShardsFromFile(path, _settings);
        var repository = new JsonRepository(shards);

        foreach (var storage in _storages)
        {
            storage.Update(repository, changes);
        }

        _jsonFileHandler.WriteModelShardsToFile(path, shards, _settings);
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

        _jsonFileHandler.WriteModelShardsToFile(path, shards, _settings);
    }

    /// <inheritdoc/>
    public void Load(string path, IModel model)
    {
        var shards = _jsonFileHandler.ReadModelShardsFromFile(path, _settings);

        var repository = new JsonRepository(shards);

        foreach (var storage in _storages)
        {
            storage.Load(repository, model);
        }
    }
}
