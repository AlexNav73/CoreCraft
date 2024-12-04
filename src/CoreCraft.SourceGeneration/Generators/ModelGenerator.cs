using CoreCraft.SourceGeneration.Extensions;

namespace CoreCraft.SourceGeneration.Generators;

internal static class ModelGenerator
{
    public static string Generate(string assemblyName, string modelName, ModelScheme modelScheme)
    {
        var @namespace = $"{assemblyName}.{modelName}";
        using var writer = new StringWriter();
        using var code = new IndentedTextWriter(writer, "    ");

        code.Preamble();

        code.WriteLine($"namespace {@namespace}");
        code.Block(() =>
        {
            code.WriteLine("using CoreCraft;");
            code.WriteLine("using CoreCraft.Core;");
            code.WriteLine("using CoreCraft.Views;");
            code.WriteLine("using CoreCraft.ChangesTracking;");
            code.WriteLine("using CoreCraft.Persistence;");
            code.WriteLine("using CoreCraft.Persistence.History;");
            code.WriteLine($"using {@namespace}.Entities;");
            code.EmptyLine();

            new ModelShardGenerator(code).Generate(modelScheme.Shards);
        });
        code.EmptyLine();

        code.WriteLine($"namespace {@namespace}.Entities");
        code.Block(() =>
        {
            new EntitiesGenerator(code).Generate(modelScheme.Shards);
        });

        return writer.ToString();
    }
}
