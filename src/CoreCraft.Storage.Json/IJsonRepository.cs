using CoreCraft.Persistence;
using CoreCraft.Persistence.History;

namespace CoreCraft.Storage.Json;

/// <summary>
///     Json extension of the base <see cref="IRepository"/> interface
/// </summary>
public interface IJsonRepository : IRepository, IHistoryRepository
{
}
