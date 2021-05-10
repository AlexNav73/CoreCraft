using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace PricingCalc.Model.Generators
{
    internal partial class ApplicationModelGenerator
    {
        public void GenerateStorages(IndentedTextWriter code, IEnumerable<ModelShard> shards)
        {
            foreach (var modelShard in shards)
            {
                DefineStorage(code, modelShard);
                EmptyLine(code);
            }
        }

        private void DefineStorage(IndentedTextWriter code, ModelShard modelShard)
        {
            code.WriteLine($"[global::System.CodeDom.Compiler.GeneratedCode(\"C# Source Generator\", \"{modelShard.Version}\")]");
            Class(code, "sealed", $"{modelShard.Name}ModelShardStorage", new[]
            {
                $"ModelShardStorage<I{modelShard.Name}ModelShard, I{modelShard.Name}ChangesFrame>"
            },
            () =>
            {
                code.WriteLine($"public {modelShard.Name}ModelShardStorage(I{modelShard.Name}ModelShard shard) : base(shard)");
                Block(code, () => { });
                EmptyLine(code);

                foreach (var collection in modelShard.Collections)
                {
                    var entity = modelShard.Entities.Single(x => x.Name == collection.Type);
                    var properties = entity.Properties.Select(x => $"new(\"{x.Name}\", typeof({x.Type}), {x.IsNullable.ToString().ToLower()})");
                    var array = string.Join(", ", properties);

                    code.WriteLine($"private static readonly Scheme _{ToCamelCase(collection.Name)}Scheme = new(new Property[] {{ {array} }});");
                }
                EmptyLine(code);

                code.WriteLine($"protected override void SaveInternal(string path, IRepository repository, I{modelShard.Name}ChangesFrame changes)");
                Block(code, () =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        code.WriteLine($"Save(repository, \"{modelShard.Name}.{collection.Name}\", changes.{collection.Name}, _{ToCamelCase(collection.Name)}Scheme);");
                    }
                    EmptyLine(code);

                    foreach (var relation in modelShard.Relations)
                    {
                        code.WriteLine($"Save(repository, \"{modelShard.Name}.{relation.Name}\", changes.{relation.Name});");
                    }
                });
                EmptyLine(code);

                code.WriteLine($"protected override void SaveInternal(string path, IRepository repository)");
                Block(code, () =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        code.WriteLine($"Save(repository, \"{modelShard.Name}.{collection.Name}\", Shard.{collection.Name}, _{ToCamelCase(collection.Name)}Scheme);");
                    }
                    EmptyLine(code);

                    foreach (var relation in modelShard.Relations)
                    {
                        code.WriteLine($"Save(repository, \"{modelShard.Name}.{relation.Name}\", Shard.{relation.Name});");
                    }
                });
                EmptyLine(code);

                code.WriteLine($"protected override void LoadInternal(string path, IRepository repository)");
                Block(code, () =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        code.WriteLine($"Load(repository, \"{modelShard.Name}.{collection.Name}\", Shard.{collection.Name}, _{ToCamelCase(collection.Name)}Scheme);");
                    }
                    EmptyLine(code);

                    foreach (var relation in modelShard.Relations)
                    {
                        var parentCollection = modelShard.Collections.Single(x => x.Type == relation.ParentType).Name;
                        var childCollection = modelShard.Collections.Single(x => x.Type == relation.ChildType).Name;

                        code.WriteLine($"Load(repository, \"{modelShard.Name}.{relation.Name}\", Shard.{relation.Name}, Shard.{parentCollection}, Shard.{childCollection});");
                    }
                });
            });
        }
    }
}
