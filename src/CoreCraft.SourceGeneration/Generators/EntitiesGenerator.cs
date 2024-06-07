using CoreCraft.SourceGeneration.Extensions;

namespace CoreCraft.SourceGeneration.Generators;

internal sealed class EntitiesGenerator(IndentedTextWriter code) : GeneratorCommon
{
    public void Generate(IEnumerable<ModelShard> shards)
    {
        code.WriteLine("using CoreCraft.Core;");
        code.EmptyLine();

        foreach (var modelShard in shards)
        {
            DefineEntities(modelShard);
            code.EmptyLine();
        }
    }

    private void DefineEntities(ModelShard modelShard)
    {
        foreach (var entity in modelShard.Collections.Select(x => x.Entity))
        {
            DefineEntityType(entity);
            code.EmptyLine();
            DefineEntityPropertiesClass(entity);
            code.EmptyLine();
        }
    }

    private void DefineEntityType(Entity entity)
    {
        code.GeneratedClassAttributes(entity.Collection.Shard.Scheme.Debug);
        code.WriteLine($"public sealed record {entity.Name}(global::System.Guid Id) : Entity(Id)");
        code.Block(() =>
        {
            code.WriteLine($"internal {entity.Name}() : this(global::System.Guid.NewGuid())");
            code.Block(() =>
            {
            });
        });
    }

    private void DefineEntityPropertiesClass(Entity entity)
    {
        code.GeneratedClassAttributes(entity.Collection.Shard.Scheme.Debug);
        code.WriteLine($"public sealed partial record {entity.PropertiesType} : Properties");
        code.Block(() =>
        {
            DefineCtor(entity);
            code.EmptyLine();

            foreach (var prop in entity.Properties)
            {
                code.WriteLine($"public {prop.Define("get; init;")}");
            }
            code.EmptyLine();

            ImplementEntityPropertiesMethods(entity);
            code.EmptyLine();
        });

        void DefineCtor(Entity entity)
        {
            code.WriteLine($"public {entity.PropertiesType}()");
            code.Block(() =>
            {
                foreach (var prop in entity.Properties.Where(x => !x.IsNullable && !string.IsNullOrEmpty(x.DefaultValue)))
                {
                    code.WriteLine($"{prop.Name} = {prop.DefaultValue};");
                }
            });
        }

        void ImplementEntityPropertiesMethods(Entity entity)
        {
            code.NoIndent(c => c.WriteLine("#if NET5_0_OR_GREATER"));
            code.WriteLine($"public override {entity.PropertiesType} ReadFrom(IPropertiesBag bag)");
            code.NoIndent(c => c.WriteLine("#else"));
            code.WriteLine("public override Properties ReadFrom(IPropertiesBag bag)");
            code.NoIndent(c => c.WriteLine("#endif"));
            code.Block(() =>
            {
                code.WriteLine($"return new {entity.PropertiesType}()");
                code.Block(() =>
                {
                    foreach (var prop in entity.Properties)
                    {
                        code.WriteLine($"{prop.Name} = bag.Read<{prop.Type}>(\"{prop.Name}\"),");
                    }
                }, true);
            });
            code.EmptyLine();

            code.WriteLine($"public override void WriteTo(IPropertiesBag bag)");
            code.Block(() =>
            {
                foreach (var prop in entity.Properties)
                {
                    code.WriteLine($"bag.Write(\"{prop.Name}\", {prop.Name});");
                }
            });
        }
    }
}
