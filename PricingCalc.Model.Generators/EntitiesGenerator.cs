using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace PricingCalc.Model.Generators
{
    internal partial class ApplicationModelGenerator
    {
        public void GenerateEntities(IndentedTextWriter code, IEnumerable<ModelShard> shards)
        {
            code.WriteLine("using PricingCalc.Model.Engine.Core;");
            code.EmptyLine();

            foreach (var modelShard in shards)
            {
                DefineEntities(code, modelShard);
                code.EmptyLine();
            }
        }

        private void DefineEntities(IndentedTextWriter code, ModelShard modelShard)
        {
            foreach (var entity in modelShard.Entities)
            {
                DefineEntityType(code, entity);
                code.EmptyLine();
                DefineEntityPropertiesClass(code, entity);
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
            code.WriteLine($"public sealed record {PropertiesType(entity)} : Properties");
            code.Block(() =>
            {
                DefineCtor(code, entity);
                code.EmptyLine();

                foreach (var prop in entity.Properties)
                {
                    code.WriteLine($"public {DefineProperty(prop, "get; init;")}");
                }
                code.EmptyLine();

                ImplementEntityPropertiesMethods(code, entity);
                code.EmptyLine();
            });

            void DefineCtor(IndentedTextWriter code, Entity entity)
            {
                code.WriteLine($"public {PropertiesType(entity)}()");
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
                code.WriteLine($"public override {PropertiesType(entity)} ReadFrom(IPropertiesBag bag)");
                code.NoIndent(c => c.WriteLine("#else"));
                code.WriteLine("public override Properties ReadFrom(IPropertiesBag bag)");
                code.NoIndent(c => c.WriteLine("#endif"));
                code.Block(() =>
                {
                    code.WriteLine($"return new {PropertiesType(entity)}()");
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
}
