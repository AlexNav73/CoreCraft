using CoreCraft.Persistence;
using CoreCraft.Persistence.History;

namespace CoreCraft.Storage.Json;

/// <summary>
///     A Json storage implementation for the domain model
/// </summary>
public interface IJsonStorage : IStorage, IHistoryStorage
{
}
