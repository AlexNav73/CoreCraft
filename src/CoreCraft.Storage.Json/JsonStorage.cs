using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Persistence;
using CoreCraft.Persistence.History;
using CoreCraft.Persistence.Lazy;
using CoreCraft.Persistence.Operations;
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
        var shards = _jsonFileHandler.ReadModelFromFile(_path, _settings);
        var repository = new JsonRepository(shards);

        foreach (var change in modelChanges.Cast<IChangesFrameEx>())
        {
            change.Do(new UpdateChangesFrameOperation(repository));
        }

        _jsonFileHandler.WriteModelToFile(_path, shards, _settings);
    }

    /// <inheritdoc/>
    public void Save(IEnumerable<IModelChanges> modelChanges)
    {
        var shards = _jsonFileHandler.ReadModelFromFile(_path, _settings);
        var repository = new JsonRepository(shards);

        foreach (var change in modelChanges)
        {
            change.Save(repository);
        }

        _jsonFileHandler.WriteModelToFile(_path, shards, _settings);
    }

    /// <inheritdoc/>
    public void Save(IEnumerable<IModelShard> modelShards)
    {
        var model = new Model.Model();
        var repository = new JsonRepository(model);

        foreach (var shard in modelShards)
        {
            shard.Save(repository);
        }

        _jsonFileHandler.WriteModelToFile(_path, model, _settings);
    }

    /// <inheritdoc/>
    public void Load(IEnumerable<IMutableModelShard> modelShards, bool force = false)
    {
        var model = _jsonFileHandler.ReadModelFromFile(_path, _settings);

        var repository = new JsonRepository(model);

        foreach (var loadable in modelShards.Where(x => force || !x.ManualLoadRequired))
        {
            loadable.Load(repository, force);
        }
    }

    /// <inheritdoc/>
    public void Load(ILazyLoader loader)
    {
        var model = _jsonFileHandler.ReadModelFromFile(_path, _settings);

        loader.Load(new JsonRepository(model));
    }

    /// <inheritdoc/>
    public IEnumerable<IModelChanges> Load(IEnumerable<IModelShard> modelShards)
    {
        var model = _jsonFileHandler.ReadModelFromFile(_path, _settings);

        var repository = new JsonRepository(model);

        return repository.LoadHistory(modelShards);
    }
}
