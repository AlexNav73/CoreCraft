using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Persistence;
using CoreCraft.Persistence.History;
using CoreCraft.Persistence.Lazy;
using CoreCraft.Persistence.Operations;
using CoreCraft.Storage.Json.Model;
using Newtonsoft.Json;

namespace CoreCraft.Storage.Json;

/// <summary>
///     A Json storage implementation for the domain model
/// </summary>
public sealed class JsonStorage : IStorage, IHistoryStorage
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
    public void Update(IEnumerable<IChangesFrame> modelChanges)
    {
        var shards = _jsonFileHandler.ReadModelShardsFromFile(_path, _settings);
        var repository = new JsonRepository(shards);

        foreach (var change in modelChanges.Cast<IChangesFrameEx>())
        {
            change.Do(new UpdateChangesFrameOperation(repository));
        }

        _jsonFileHandler.WriteModelShardsToFile(_path, shards, _settings);
    }

    /// <inheritdoc/>
    public void Save(IEnumerable<IModelChanges> modelChanges)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void Save(IEnumerable<IModelShard> modelShards)
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
    public void Load(IEnumerable<IMutableModelShard> modelShards, bool force = false)
    {
        var shards = _jsonFileHandler.ReadModelShardsFromFile(_path, _settings);

        var repository = new JsonRepository(shards);

        foreach (var loadable in modelShards.Where(x => force || !x.ManualLoadRequired))
        {
            loadable.Load(repository, force);
        }
    }

    /// <inheritdoc/>
    public void Load(ILazyLoader loader)
    {
        var shards = _jsonFileHandler.ReadModelShardsFromFile(_path, _settings);

        loader.Load(new JsonRepository(shards));
    }

    /// <inheritdoc/>
    public IEnumerable<IModelChanges> Load(IEnumerable<IModelShard> modelShards)
    {
        throw new NotImplementedException();
    }
}
