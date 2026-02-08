using CoreCraft.SourceGeneration.Extensions;

namespace CoreCraft.SourceGeneration.Generators;

internal class ModelGenerator(
    IndentedTextWriter code,
    ModelShardGenerator modelShardGenerator,
    EntitiesGenerator entitiesGenerator)
{
    protected readonly IndentedTextWriter Code = code;

    public void Generate(string assemblyName, string modelName, ModelScheme modelScheme)
    {
        var @namespace = $"{assemblyName}.{modelName}";

        Code.Preamble();

        Code.WriteLine($"namespace {@namespace}");
        Code.Block(() =>
        {
            EmitModelUsingDirectives();

            Code.WriteLine($"using {@namespace}.Entities;");
            Code.EmptyLine();

            modelShardGenerator.Generate(modelScheme.Shards);
        });
        Code.EmptyLine();

        Code.WriteLine($"namespace {@namespace}.Entities");
        Code.Block(() =>
        {
            EmitEntitiesUsingDirectives();
            Code.EmptyLine();

            entitiesGenerator.Generate(modelScheme.Shards);
        });
    }

    protected virtual void EmitModelUsingDirectives()
    {
        Code.WriteLine("using CoreCraft;");
        Code.WriteLine("using CoreCraft.Core;");
        Code.WriteLine("using CoreCraft.Views;");
        Code.WriteLine("using CoreCraft.Features.CoW;");
        Code.WriteLine("using CoreCraft.Features.Tracking;");
        Code.WriteLine("using CoreCraft.ChangesTracking;");
        Code.WriteLine("using CoreCraft.Persistence;");
        Code.WriteLine("using CoreCraft.Persistence.History;");
    }

    protected virtual void EmitEntitiesUsingDirectives()
    {
        Code.WriteLine("using CoreCraft.Core;");
    }
}
