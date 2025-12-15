using CoreCraft.SourceGeneration;
using CoreCraft.SourceGeneration.Extensions;
using CoreCraft.SourceGeneration.Generators;

namespace CoreCraft.Storage.Linq2Db.Generators;

internal class LazyEntitiesGenerator(IndentedTextWriter code) : EntitiesGenerator(code)
{
    protected override void DefineEntityType(Entity entity)
    {
        code.WriteLine($"partial record {entity.Name} : Entity");
        code.Block(() =>
        {
        });
    }

    protected override void DefineEntityPropertiesClass(Entity entity)
    {
        code.WriteLine($"partial record {entity.PropertiesType} : Properties, IHaveEntityId<{entity.Name}>");
        code.Block(() =>
        {
            code.WriteLine($"public {DefineProperty(entity.Name, "EntityId", "get;")} = new {entity.Name}();");
        });
    }
}
