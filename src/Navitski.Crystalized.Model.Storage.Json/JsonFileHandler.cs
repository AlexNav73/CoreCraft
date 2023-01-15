using Navitski.Crystalized.Model.Storage.Json.Model;
using Newtonsoft.Json;

namespace Navitski.Crystalized.Model.Storage.Json;

internal class JsonFileHandler : IJsonFileHandler
{
    public IList<ModelShard> ReadModelShardsFromFile(string path)
    {
        if (File.Exists(path))
        {
            using var fileReader = File.OpenText(path);
            var reader = new JsonTextReader(fileReader);
            var serializer = CreateSerializer();

            return serializer.Deserialize<IList<ModelShard>>(reader) ?? new List<ModelShard>();
        }

        return new List<ModelShard>();
    }

    public void WriteModelShardsToFile(string path, IList<ModelShard> shards)
    {
        using var sw = new StreamWriter(path);
        using var writer = new JsonTextWriter(sw);
        var serializer = CreateSerializer();

        serializer.Serialize(writer, shards);
    }

    private static JsonSerializer CreateSerializer()
    {
        return JsonSerializer.Create(new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        });
    }
}
