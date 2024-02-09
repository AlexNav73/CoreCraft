namespace CoreCraft.Generators;

internal partial class ApplicationModelGenerator
{
    public void GenerateEntities(IndentedTextWriter code, IEnumerable<ModelShard> shards)
    {
        code.WriteLine("using CoreCraft.Core;");
        code.EmptyLine();

        foreach (var modelShard in shards)
        {
            DefineEntities(code, modelShard);
            code.EmptyLine();
        }
    }

    private void DefineEntities(IndentedTextWriter code, ModelShard modelShard)
    {
        foreach (var entity in modelShard.Collections.Select(x => x.Entity))
        {
            DefineEntityType(code, entity);
            code.EmptyLine();
            DefineEntityPropertiesClass(code, entity);
            code.EmptyLine();
        }
    }

    private void DefineEntityType(IndentedTextWriter code, Entity entity)
    {
        code.GeneratedClassAttributes();
        code.WriteLine($"public sealed record {entity.Name}(global::System.Guid Id) : Entity(Id)");
        code.Block(() =>
        {
            code.WriteLine($"internal {entity.Name}() : this(global::System.Guid.NewGuid())");
            code.Block(() =>
            {
            });
        });
    }

    private void DefineEntityPropertiesClass(IndentedTextWriter code, Entity entity)
    {
        code.GeneratedClassAttributes();
        code.WriteLine($"public sealed partial record {entity.PropertiesType} : Properties");
        code.Block(() =>
        {
            DefineCtor(code, entity);
            code.EmptyLine();

            foreach (var prop in entity.Properties)
            {
                code.WriteLine($"public {prop.Define("get; init;")}");
            }
            code.EmptyLine();

            ImplementEntityPropertiesMethods(code, entity);
            code.EmptyLine();
        });

        void DefineCtor(IndentedTextWriter code, Entity entity)
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

        void ImplementEntityPropertiesMethods(IndentedTextWriter code, Entity entity)
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
