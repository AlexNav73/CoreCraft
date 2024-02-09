﻿using System.Collections.Immutable;
using CoreCraft.Generators.Dto;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

namespace CoreCraft.Generators;

[Generator(LanguageNames.CSharp)]
internal partial class ApplicationModelGenerator : GeneratorBase
{
    protected override void ExecuteInternal(
        SourceProductionContext context,
        string assemblyName,
        ImmutableArray<(string name, string content)> files)
    {
        var options = new JsonSerializerSettings();
        var serializer = JsonSerializer.Create(options);

        foreach (var file in files)
        {
            ModelScheme modelScheme = null;

            using (var stringStream = new StringReader(file.content))
            using (var reader = new JsonTextReader(stringStream))
            {
                modelScheme = DtoConverter.Convert(serializer.Deserialize<ModelSchemeDto>(reader));
            }

            if (modelScheme == null)
            {
                throw new InvalidOperationException($"Failed to deserialize model file [{file.name}]");
            }

            using (var writer = new StringWriter())
            using (var code = new IndentedTextWriter(writer, "    "))
            {
                code.Preamble();
                Generate(assemblyName, file.name, code, modelScheme);

                AddSourceFile(context, file.name, writer.ToString());
            }
        }
    }

    private void Generate(string assemblyName, string modelName, IndentedTextWriter code, ModelScheme modelScheme)
    {
        var @namespace = $"{assemblyName}.{modelName}";

        code.WriteLine($"namespace {@namespace}");
        code.Block(() =>
        {
            code.WriteLine("using CoreCraft.Core;");
            code.WriteLine("using CoreCraft.ChangesTracking;");
            code.WriteLine("using CoreCraft.Persistence;");
            code.WriteLine($"using {@namespace}.Entities;");
            code.EmptyLine();

            GenerateModelShards(code, modelScheme.Shards);
        });
        code.EmptyLine();

        code.WriteLine($"namespace {@namespace}.Entities");
        code.Block(() =>
        {
            GenerateEntities(code, modelScheme.Shards);
        });
    }

    private static string DefineProperty(string type, string name, string accessors = "get; private set;")
    {
        return string.Join(" ", type, name, "{", accessors, "}").Trim();
    }

    private static string ToCamelCase(string value)
    {
        if (value.Length > 1)
        {
            return $"{char.ToLower(value[0])}{value.Substring(1)}";
        }

        return value;
    }
}
