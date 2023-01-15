using Navitski.Crystalized.Model.Storage.Json.Model;

namespace Navitski.Crystalized.Model.Storage.Json;

internal interface IJsonFileHandler
{
    IList<ModelShard> ReadModelShardsFromFile(string path);

    void WriteModelShardsToFile(string path, IList<ModelShard> shards);
}
