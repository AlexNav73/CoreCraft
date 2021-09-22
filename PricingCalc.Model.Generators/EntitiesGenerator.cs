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
                DefineEntityType(code, entity, modelShard);
                code.EmptyLine();
                DefineEntityPropertiesInterface(code, entity, modelShard);
                code.EmptyLine();
                DefineEntityPropertiesClass(code, entity);
            }
        }

        private void DefineEntityType(IndentedTextWriter code, Entity entity, ModelShard shard)
        {
            code.GeneratedInterfaceAttributes();
            code.Interface(shard.IsInternal, $"I{entity.Name}", new[] { "IEntity", $"ICopy<I{entity.Name}>" }, () => { });
            code.EmptyLine();

            code.GeneratedClassAttributes();
            code.DebuggerDisplay("Id = {Id}");
            code.WriteLine($"internal sealed record {entity.Name}(global::System.Guid Id) : I{entity.Name}");
            code.Block(() =>
            {
                code.WriteLine($"internal {entity.Name}() : this(global::System.Guid.NewGuid())");
                code.Block(() =>
                {
                });
                code.EmptyLine();

                code.WriteLine($"public I{entity.Name} Copy()");
                code.Block(() =>
                {
                    code.WriteLine("return this with { };");
                });
            });
        }

        private void DefineEntityPropertiesInterface(IndentedTextWriter code, Entity entity, ModelShard shard)
        {
            code.GeneratedInterfaceAttributes();
            code.Interface(shard.IsInternal, $"I{PropertiesType(entity)}", new[]
            {
                $"IEntityProperties",
                $"ICopy<I{PropertiesType(entity)}>",
                $"global::System.IEquatable<I{PropertiesType(entity)}>"
            },
            () =>
            {
                foreach (var prop in entity.Properties)
                {
                    code.WriteLine(DefineProperty(prop, "get; set;"));
                }
            });
        }

        private void DefineEntityPropertiesClass(IndentedTextWriter code, Entity entity)
        {
            code.GeneratedClassAttributes();
            code.WriteLine($"internal sealed record {PropertiesType(entity)} : I{PropertiesType(entity)}");
            code.Block(() =>
            {
                foreach (var prop in entity.Properties)
                {
                    code.WriteLine($"public {DefineProperty(prop, "get; set;")}");
                }
                code.EmptyLine();

                DefineCtor(code, entity);
                code.EmptyLine();
                ImplementCopyInterface(code, entity);
                code.EmptyLine();
                ImplementEntityPropertiesInterface(code, entity);
                code.EmptyLine();
                ImplementEquatableInterface(code, entity);
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

            void ImplementCopyInterface(IndentedTextWriter code, Entity entity)
            {
                code.WriteLine($"public I{PropertiesType(entity)} Copy()");
                code.Block(() =>
                {
                    code.WriteLine("return this with { };");
                });
            }

            void ImplementEntityPropertiesInterface(IndentedTextWriter code, Entity entity)
            {
                code.WriteLine($"public void ReadFrom(IPropertiesBag bag)");
                code.Block(() =>
                {
                    foreach (var prop in entity.Properties)
                    {
                        code.WriteLine($"{prop.Name} = bag.Read<{prop.Type}>(\"{prop.Name}\");");
                    }
                });
                code.EmptyLine();

                code.WriteLine($"public void WriteTo(IPropertiesBag bag)");
                code.Block(() =>
                {
                    foreach (var prop in entity.Properties)
                    {
                        code.WriteLine($"bag.Write(\"{prop.Name}\", {prop.Name});");
                    }
                });
            }

            void ImplementEquatableInterface(IndentedTextWriter code, Entity entity)
            {
                code.WriteLine($"public bool Equals(I{PropertiesType(entity)}? other)");
                code.Block(() =>
                {
                    code.WriteLine($"return Equals(({PropertiesType(entity)}?)other);");
                });
            }
        }
    }
}
