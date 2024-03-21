using Newtonsoft.Json;

namespace CoreCraft.Storage.Json;

internal interface IJsonFileHandler
{
    Model.Model ReadModelFromFile(
        string path,
        JsonSerializerSettings? settings);

    void WriteModelToFile(
        string path,
        Model.Model shards,
        JsonSerializerSettings? settings);
}
