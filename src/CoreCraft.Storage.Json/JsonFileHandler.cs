using CoreCraft.Storage.Json.Model;
using Newtonsoft.Json;

namespace CoreCraft.Storage.Json;

internal class JsonFileHandler : IJsonFileHandler
{
    public IList<ModelShard> ReadModelShardsFromFile(
        string path,
        JsonSerializerSettings? settings)
    {
        if (File.Exists(path))
        {
            using var fileReader = File.OpenText(path);
            var reader = new JsonTextReader(fileReader);
            var serializer = CreateSerializer(settings);

            return serializer.Deserialize<IList<ModelShard>>(reader) ?? new List<ModelShard>();
        }

        return new List<ModelShard>();
    }

    public void WriteModelShardsToFile(
        string path,
        IList<ModelShard> shards,
        JsonSerializerSettings? settings)
    {
        using var sw = new StreamWriter(path);
        using var writer = new JsonTextWriter(sw);
        var serializer = CreateSerializer(settings);

        serializer.Serialize(writer, shards);
    }

    private static JsonSerializer CreateSerializer(JsonSerializerSettings? settings)
    {
        settings ??= new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        settings.TypeNameHandling = settings.TypeNameHandling switch
        {
            TypeNameHandling.None => TypeNameHandling.Auto,
            TypeNameHandling.Auto or TypeNameHandling.Objects or TypeNameHandling.All => settings.TypeNameHandling,
            TypeNameHandling.Arrays => TypeNameHandling.All,
            _ => throw new NotSupportedException($"TypeNameHandling option with {settings.TypeNameHandling} value is not supported")
        };

        return JsonSerializer.Create(settings);
    }
}
