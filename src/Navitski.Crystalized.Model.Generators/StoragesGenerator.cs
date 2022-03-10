namespace Navitski.Crystalized.Model.Generators;

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
        code.GeneratedClassAttributes();
        code.Class("sealed", $"{modelShard.Name}ModelShardStorage", new[] { $"ModelShardStorage" }, () =>
        {
            foreach (var collection in modelShard.Collections)
            {
                var entity = modelShard.Entities.Single(x => x.Name == collection.Type);
                var properties = entity.Properties.Select(x => $"new(\"{x.Name}\", typeof({x.Type}), {x.IsNullable.ToString().ToLower()})");
                var array = string.Join(", ", properties);

                code.WriteLine($"private static readonly Scheme _{ToCamelCase(collection.Name)}Scheme = new(new Property[] {{ {array} }});");
            }
            code.EmptyLine();

            code.WriteLine("public override void Save(IRepository repository, IModel model, IModelChanges changes)");
            code.Block(() =>
            {
                code.WriteLine($"if (changes.TryGetFrame<I{modelShard.Name}ChangesFrame>(out var frame))");
                code.Block(() =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        code.WriteLine($"Save(repository, \"{modelShard.Name}.{collection.Name}\", frame.{collection.Name}, _{ToCamelCase(collection.Name)}Scheme);");
                    }
                    code.EmptyLine();

                    foreach (var relation in modelShard.Relations)
                    {
                        code.WriteLine($"Save(repository, \"{modelShard.Name}.{relation.Name}\", frame.{relation.Name});");
                    }
                });
            });
            code.EmptyLine();

            code.WriteLine("public override void Save(IRepository repository, IModel model)");
            code.Block(() =>
            {
                code.WriteLine($"var shard = model.Shard<I{modelShard.Name}ModelShard>();");
                code.EmptyLine();

                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine($"Save(repository, \"{modelShard.Name}.{collection.Name}\", shard.{collection.Name}, _{ToCamelCase(collection.Name)}Scheme);");
                }
                code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine($"Save(repository, \"{modelShard.Name}.{relation.Name}\", shard.{relation.Name});");
                }
            });
            code.EmptyLine();

            code.WriteLine("public override void Load(IRepository repository, IModel model)");
            code.Block(() =>
            {
                code.WriteLine($"var shard = model.Shard<IMutable{modelShard.Name}ModelShard>();");
                code.EmptyLine();

                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine($"Load(repository, \"{modelShard.Name}.{collection.Name}\", shard.{collection.Name}, _{ToCamelCase(collection.Name)}Scheme);");
                }
                code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    var parentCollection = modelShard.Collections.Single(x => x.Type == relation.ParentType).Name;
                    var childCollection = modelShard.Collections.Single(x => x.Type == relation.ChildType).Name;

                    code.WriteLine($"Load(repository, \"{modelShard.Name}.{relation.Name}\", shard.{relation.Name}, shard.{parentCollection}, shard.{childCollection});");
                }
            });
        });
    }
}
