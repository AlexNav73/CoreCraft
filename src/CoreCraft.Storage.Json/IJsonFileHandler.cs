using CoreCraft.Storage.Json.Model;
using Newtonsoft.Json;

namespace CoreCraft.Storage.Json;

internal interface IJsonFileHandler
{
    IList<ModelShard> ReadModelShardsFromFile(
        string path,
        JsonSerializerSettings? settings);

    void WriteModelShardsToFile(
        string path,
        IList<ModelShard> shards,
        JsonSerializerSettings? settings);
}
