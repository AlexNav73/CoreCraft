using CoreCraft.Persistence;
using CoreCraft.Storage.Json.Model;
using Newtonsoft.Json;

namespace CoreCraft.Storage.Json;

/// <summary>
///     A Json storage implementation for the domain model
/// </summary>
public sealed class JsonStorage : IStorage
{
    private readonly string _path;
    private readonly JsonSerializerSettings? _settings;
    private readonly IJsonFileHandler _jsonFileHandler;

    /// <summary>
    ///     Ctor
    /// </summary>
    public JsonStorage(string path, JsonSerializerSettings? options = null)
        : this(path, new JsonFileHandler(), options)
    {
    }

    internal JsonStorage(
        string path,
        IJsonFileHandler jsonFileHandler,
        JsonSerializerSettings? options = null)
    {
        _path = path;
        _jsonFileHandler = jsonFileHandler;
        _settings = options;
    }

    /// <inheritdoc/>
    public void Update(IEnumerable<ICanBeSaved> modelChanges)
    {
        var shards = _jsonFileHandler.ReadModelShardsFromFile(_path, _settings);
        var repository = new JsonRepository(shards);

        foreach (var change in modelChanges)
        {
            change.Save(repository);
        }

        _jsonFileHandler.WriteModelShardsToFile(_path, shards, _settings);
    }

    /// <inheritdoc/>
    public void Save(IEnumerable<ICanBeSaved> modelShards)
    {
        var shards = new List<ModelShard>();
        var repository = new JsonRepository(shards);

        foreach (var shard in modelShards)
        {
            shard.Save(repository);
        }

        _jsonFileHandler.WriteModelShardsToFile(_path, shards, _settings);
    }

    /// <inheritdoc/>
    public void Load(IEnumerable<ICanBeLoaded> modelShards)
    {
        var shards = _jsonFileHandler.ReadModelShardsFromFile(_path, _settings);

        var repository = new JsonRepository(shards);

        foreach (var loadable in modelShards)
        {
            loadable.Load(repository);
        }
    }
}
