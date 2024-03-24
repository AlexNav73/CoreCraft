using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Persistence;
using CoreCraft.Persistence.History;

namespace CoreCraft.Storage.Json;

/// <summary>
///     Json extension of the base <see cref="IRepository"/> interface
/// </summary>
public interface IJsonRepository : IRepository, IHistoryRepository
{
    /// <summary>
    ///     TODO: write documentation
    /// </summary>
    /// <param name="modelShards"></param>
    /// <returns></returns>
    IEnumerable<IModelChanges> RestoreHistory(IEnumerable<IModelShard> modelShards);
}
