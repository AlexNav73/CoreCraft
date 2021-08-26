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
                code.EmptyLine();
            }
        }

        private void DefineStorage(IndentedTextWriter code, ModelShard modelShard)
        {
            code.GeneratedCodeAttribute();
            code.Class("sealed", $"{modelShard.Name}ModelShardStorage", new[]
            {
                $"ModelShardStorage<I{modelShard.Name}ModelShard, I{modelShard.Name}ChangesFrame>"
            },
            () =>
            {
                code.WriteLine($"public {modelShard.Name}ModelShardStorage(I{modelShard.Name}ModelShard shard) : base(shard)");
                code.Block(() => { });
                code.EmptyLine();

                foreach (var collection in modelShard.Collections)
                {
                    var entity = modelShard.Entities.Single(x => x.Name == collection.Type);
                    var properties = entity.Properties.Select(x => $"new(\"{x.Name}\", typeof({x.Type}), {x.IsNullable.ToString().ToLower()})");
                    var array = string.Join(", ", properties);

                    code.WriteLine($"private static readonly Scheme _{ToCamelCase(collection.Name)}Scheme = new(new Property[] {{ {array} }});");
                }
                code.EmptyLine();

                code.WriteLine($"protected override void SaveInternal(string path, IRepository repository, I{modelShard.Name}ChangesFrame changes)");
                code.Block(() =>
                {
                    code.WriteLine($"repository.UpdateVersionInfo(\"{modelShard.Name}\", \"{modelShard.Version}\");");
                    code.EmptyLine();

                    foreach (var collection in modelShard.Collections)
                    {
                        code.WriteLine($"Save(repository, \"{modelShard.Name}.{collection.Name}\", changes.{collection.Name}, _{ToCamelCase(collection.Name)}Scheme);");
                    }
                    code.EmptyLine();

                    foreach (var relation in modelShard.Relations)
                    {
                        code.WriteLine($"Save(repository, \"{modelShard.Name}.{relation.Name}\", changes.{relation.Name});");
                    }
                });
                code.EmptyLine();

                code.WriteLine($"protected override void SaveInternal(string path, IRepository repository)");
                code.Block(() =>
                {
                    code.WriteLine($"repository.UpdateVersionInfo(\"{modelShard.Name}\", \"{modelShard.Version}\");");
                    code.EmptyLine();

                    foreach (var collection in modelShard.Collections)
                    {
                        code.WriteLine($"Save(repository, \"{modelShard.Name}.{collection.Name}\", Shard.{collection.Name}, _{ToCamelCase(collection.Name)}Scheme);");
                    }
                    code.EmptyLine();

                    foreach (var relation in modelShard.Relations)
                    {
                        code.WriteLine($"Save(repository, \"{modelShard.Name}.{relation.Name}\", Shard.{relation.Name});");
                    }
                });
                code.EmptyLine();

                code.WriteLine($"protected override void LoadInternal(string path, IRepository repository)");
                code.Block(() =>
                {
                    code.WriteLine($"repository.Migrate(\"{modelShard.Name}\", new Version({modelShard.Version.Replace(".", ", ")}));");
                    code.EmptyLine();

                    foreach (var collection in modelShard.Collections)
                    {
                        code.WriteLine($"Load(repository, \"{modelShard.Name}.{collection.Name}\", Shard.{collection.Name}, _{ToCamelCase(collection.Name)}Scheme);");
                    }
                    code.EmptyLine();

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
