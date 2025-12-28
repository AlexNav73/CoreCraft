using CoreCraft.SourceGeneration.Generators;

namespace CoreCraft.Storage.Linq2Db.Generators;

internal sealed class LazyModelGenerator(
    IndentedTextWriter code,
    ModelShardGenerator modelShardGenerator,
    EntitiesGenerator entitiesGenerator) : ModelGenerator(code, modelShardGenerator, entitiesGenerator)
{
    protected override void EmitModelUsingDirectives()
    {
        Code.WriteLine("using CoreCraft.Core;");
        Code.WriteLine("using CoreCraft.ChangesTracking;");
        Code.WriteLine("using CoreCraft.Persistence;");
        Code.WriteLine("using CoreCraft.Persistence.History;");
        Code.WriteLine("using CoreCraft.Storage.Linq2Db;");
        Code.WriteLine("using CoreCraft.Storage.Linq2Db.Features;");
        Code.WriteLine("using LinqToDB;");
        Code.WriteLine("using LinqToDB.Data;");
        Code.WriteLine("using LinqToDB.Mapping;");
        Code.WriteLine("using LinqToDB.Interceptors;");
    }

    protected override void EmitEntitiesUsingDirectives()
    {
        code.WriteLine("using CoreCraft.Core;");
        Code.WriteLine("using CoreCraft.Storage.Linq2Db;");
    }
}
