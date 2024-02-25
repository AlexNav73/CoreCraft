using Newtonsoft.Json;

namespace CoreCraft.SourceGeneration.Serialization;

internal static class ModelSchemeReader
{
    public static ModelScheme Read(JsonSerializer serializer, string content)
    {
        using var stringStream = new StringReader(content);
        using var reader = new JsonTextReader(stringStream);
        
        return DtoConverter.Convert(serializer.Deserialize<ModelSchemeDto>(reader));
    }
}
