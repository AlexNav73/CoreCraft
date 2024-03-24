using System.Collections.Immutable;
using CoreCraft.SourceGeneration;
using CoreCraft.SourceGeneration.Generators;
using CoreCraft.SourceGeneration.Serialization;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

namespace CoreCraft.Generators;

[Generator(LanguageNames.CSharp)]
internal sealed class ApplicationModelGenerator : GeneratorBase
{
    protected override void ExecuteInternal(
        SourceProductionContext context,
        string assemblyName,
        ImmutableArray<(string name, string content)> files)
    {
        var options = new JsonSerializerSettings();
        var serializer = JsonSerializer.Create(options);

        foreach (var (name, content) in files)
        {
            ModelScheme modelScheme = ModelSchemeReader.Read(serializer, content);

            if (modelScheme == null)
            {
                throw new InvalidOperationException($"Failed to deserialize model file [{name}]");
            }
            
            var code = ModelGenerator.Generate(assemblyName, name, modelScheme);

            AddSourceFile(context, name, code);
        }
    }
}
