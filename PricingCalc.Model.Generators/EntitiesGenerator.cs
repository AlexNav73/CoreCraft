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
            EmptyLine(code);

            foreach (var modelShard in shards)
            {
                DefineEntities(code, modelShard);
                EmptyLine(code);
            }
        }

        private void DefineEntities(IndentedTextWriter code, ModelShard modelShard)
        {
            foreach (var entity in modelShard.Entities)
            {
                DefineEntityType(code, entity, modelShard.Version);
                EmptyLine(code);
                DefineEntityPropertiesInterface(code, entity, modelShard.Version);
                EmptyLine(code);
                DefineEntityPropertiesClass(code, entity, modelShard.Version);
            }
        }

        private void DefineEntityType(IndentedTextWriter code, Entity entity, string version)
        {
            code.WriteLine($"[global::System.CodeDom.Compiler.GeneratedCode(\"C# Source Generator\", \"{version}\")]");
            Interface(code, $"I{entity.Name}", new[] { "IEntity", $"ICopy<I{entity.Name}>" }, () => { });
            EmptyLine(code);

            code.WriteLine($"[global::System.CodeDom.Compiler.GeneratedCode(\"C# Source Generator\", \"{version}\")]");
            code.WriteLine("[global::System.Diagnostics.DebuggerDisplay(\"Id = {Id}\")]");
            code.WriteLine($"internal sealed record {entity.Name} : I{entity.Name}");
            Block(code, () =>
            {
                code.WriteLine($"public {Property("global::System.Guid", "Id", "get; internal set;")}");
                EmptyLine(code);

                code.WriteLine($"public I{entity.Name} Copy()");
                Block(code, () =>
                {
                    code.WriteLine("return this with { Id = Id };");
                });
            });
        }

        private void DefineEntityPropertiesInterface(IndentedTextWriter code, Entity entity, string version)
        {
            code.WriteLine($"[global::System.CodeDom.Compiler.GeneratedCode(\"C# Source Generator\", \"{version}\")]");
            Interface(code, $"I{EntityPropertiesType(entity.Name)}", new[]
            {
                $"IEntityProperties",
                $"ICopy<I{EntityPropertiesType(entity.Name)}>",
                $"global::System.IEquatable<I{EntityPropertiesType(entity.Name)}>"
            },
            () =>
            {
                foreach (var prop in entity.Properties)
                {
                    code.WriteLine(DefineProperty(prop, "get; set;"));
                }
            });
        }

        private void DefineEntityPropertiesClass(IndentedTextWriter code, Entity entity, string version)
        {
            code.WriteLine($"[global::System.CodeDom.Compiler.GeneratedCode(\"C# Source Generator\", \"{version}\")]");
            Class(code, "sealed", $"{EntityPropertiesType(entity.Name)}", new[] { $"I{EntityPropertiesType(entity.Name)}" }, () =>
            {
                foreach (var prop in entity.Properties)
                {
                    code.WriteLine($"public {DefineProperty(prop, "get; set;")}");
                }
                EmptyLine(code);

                DefineCtor(code, entity);
                EmptyLine(code);
                ImplementCopyInterface(code, entity);
                EmptyLine(code);
                ImplementEntityPropertiesInterface(code, entity);
                EmptyLine(code);
                ImplementEquatableInterface(code, entity);
            });

            void DefineCtor(IndentedTextWriter code, Entity entity)
            {
                code.WriteLine($"public {EntityPropertiesType(entity.Name)}()");
                Block(code, () =>
                {
                    foreach (var prop in entity.Properties.Where(x => !x.IsNullable && !string.IsNullOrEmpty(x.DefaultValue)))
                    {
                        code.WriteLine($"{prop.Name} = {prop.DefaultValue};");
                    }
                });
            }

            void ImplementCopyInterface(IndentedTextWriter code, Entity entity)
            {
                code.WriteLine($"public I{EntityPropertiesType(entity.Name)} Copy()");
                Block(code, () =>
                {
                    code.WriteLine($"return new {EntityPropertiesType(entity.Name)}()");
                    Block(code, () =>
                    {
                        foreach (var prop in entity.Properties)
                        {
                            code.WriteLine($"{prop.Name} = {prop.Name},");
                        }
                    }, true);
                });
            }

            void ImplementEntityPropertiesInterface(IndentedTextWriter code, Entity entity)
            {
                code.WriteLine($"public void ReadFrom(IPropertiesBag bag)");
                Block(code, () =>
                {
                    foreach (var prop in entity.Properties)
                    {
                        code.WriteLine($"{prop.Name} = bag.Read<{prop.Type}>(\"{prop.Name}\");");
                    }
                });
                EmptyLine(code);

                code.WriteLine($"public void WriteTo(IPropertiesBag bag)");
                Block(code, () =>
                {
                    foreach (var prop in entity.Properties)
                    {
                        code.WriteLine($"bag.Write(\"{prop.Name}\", {prop.Name});");
                    }
                });
            }

            void ImplementEquatableInterface(IndentedTextWriter code, Entity entity)
            {
                code.WriteLine($"public bool Equals(I{EntityPropertiesType(entity.Name)}? other)");
                Block(code, () =>
                {
                    var isNotNull = "return other != null";
                    var properties = entity.Properties.Select(GeneratePropertyEqualityComparison);

                    code.WriteLine($"{string.Join(" && ", new[] { isNotNull }.Concat(properties))};");
                });
            }
        }
    }
}
