namespace CoreCraft.Generators;

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
        code.Class(modelShard.Visibility, "sealed", $"{modelShard.Name}ModelShardStorage", new[] { $"ModelShardStorage" }, () =>
        {
            foreach (var collection in modelShard.Collections)
            {
                var entity = modelShard.Entities.Single(x => x.Name == collection.EntityType);
                var properties = entity.Properties.Select(x => $"new(\"{x.Name}\", typeof({x.Type}), {x.IsNullable.ToString().ToLower()})");
                var array = string.Join(", ", properties);

                code.WriteLine($"internal static readonly CollectionInfo {collection.Name}Info = new(\"{modelShard.Name}\", \"{collection.Name}\", new Property[] {{ {array} }});");
            }
            code.EmptyLine();

            foreach (var relation in modelShard.Relations)
            {
                code.WriteLine($"internal static readonly RelationInfo {relation.Name}Info = new(\"{modelShard.Name}\", \"{relation.Name}\");");
            }
            code.EmptyLine();

            code.WriteLine("public override void Update(IRepository repository, IModelChanges changes)");
            code.Block(() =>
            {
                code.WriteLine($"if (changes.TryGetFrame<I{modelShard.Name}ChangesFrame>(out var frame))");
                code.Block(() =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        code.WriteLine($"Update(repository, {collection.Name}Info, frame.{collection.Name});");
                    }
                    code.EmptyLine();

                    foreach (var relation in modelShard.Relations)
                    {
                        code.WriteLine($"Update(repository, {relation.Name}Info, frame.{relation.Name});");
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
                    code.WriteLine($"Save(repository, {collection.Name}Info, shard.{collection.Name});");
                }
                code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine($"Save(repository, {relation.Name}Info, shard.{relation.Name});");
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
                    code.WriteLine($"Load(repository, {collection.Name}Info, shard.{collection.Name});");
                }
                code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    var parentCollection = modelShard.Collections.Single(x => x.EntityType == relation.ParentType).Name;
                    var childCollection = modelShard.Collections.Single(x => x.EntityType == relation.ChildType).Name;

                    code.WriteLine($"Load(repository, {relation.Name}Info, shard.{relation.Name}, shard.{parentCollection}, shard.{childCollection});");
                }
            });
        });
    }
}
