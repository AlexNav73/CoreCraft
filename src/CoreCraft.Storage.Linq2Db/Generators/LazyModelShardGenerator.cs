using CoreCraft.SourceGeneration;
using CoreCraft.SourceGeneration.Extensions;
using CoreCraft.SourceGeneration.Generators;

namespace CoreCraft.Storage.Linq2Db.Generators;

internal sealed class LazyModelShardGenerator(IndentedTextWriter code)
    : ModelShardGenerator(code)
{
    protected override bool ContainsFeature(ModelShard modelShard)
    {
        return modelShard.Features.Contains("linq2db");
    }

    protected override void DefineModelShardInterface(ModelShard modelShard, bool isMutable)
    {
        var mutability = isMutable ? "Mutable" : string.Empty;

        Code.GeneratedInterfaceAttributes();
        Code.Interface($"I{mutability}{modelShard.Name}ModelShard", [isMutable ? "IMutableModelShard" : "IModelShard"], () =>
        {
            foreach (var collection in modelShard.Collections)
            {
                Code.WriteLine(DefineProperty($"I{mutability}Lazy{collection.Type}", collection.Name, "get;"));
            }

            Code.EmptyLine();

            foreach (var relation in modelShard.Relations)
            {
                Code.WriteLine(DefineProperty($"I{mutability}Lazy{relation.Type}", relation.Name, "get;"));
            }
        });
    }

    protected override void DefineModelShardClass(ModelShard modelShard)
    {
        Code.GeneratedClassAttributes(modelShard.Scheme.Debug);
        Code.Class(modelShard.Visibility, "sealed partial", $"{modelShard.Name}ModelShard", [$"I{modelShard.Name}ModelShard"], () =>
        {
            DefineCtor(modelShard);
            Code.EmptyLine();
            DefineConversionCtor(modelShard);
            Code.EmptyLine();
            ImplementModelShardInterface(modelShard);
            Code.EmptyLine();
            ImplementSaveMethod(modelShard);
        });

        void DefineCtor(ModelShard modelShard)
        {
            Code.WriteLine($"public {modelShard.Name}ModelShard(DataConnection db)");
            Code.Block(() =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    Code.WriteLine($"{collection.Name} = new Lazy{collection.Type}(");
                    Code.WithIndent(c =>
                    {
                        c.WriteLine($"{modelShard.Name}ModelShardInfo.{collection.Name}Info,");
                        c.WriteLine($"db.GetTable<{collection.Entity.PropertiesType}>());");
                    });
                }
                Code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    Code.WriteLine($"{relation.Name} = new Lazy{relation.Type}(");
                    Code.WithIndent(c =>
                    {
                        c.WriteLine($"{modelShard.Name}ModelShardInfo.{relation.Name}Info,");
                        Code.WriteLine($"new {relation.ParentRelationType}<{relation.Parent.Entity.Name}, {relation.Child.Entity.Name}>(),");
                        Code.WriteLine($"new {relation.ChildRelationType}<{relation.Child.Entity.Name}, {relation.Parent.Entity.Name}>());");
                    });
                }
            });
        }

        void DefineConversionCtor(ModelShard modelShard)
        {
            Code.WriteLine($"internal {modelShard.Name}ModelShard(IMutable{modelShard.Name}ModelShard mutable)");
            Code.Block(() =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    Code.WriteLine($"{collection.Name} = ((IMutableState<ILazy{collection.Type}>)mutable.{collection.Name}).AsReadOnly();");
                }
                Code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    Code.WriteLine($"{relation.Name} = ((IMutableState<ILazy{relation.Type}>)mutable.{relation.Name}).AsReadOnly();");
                }
            });
        }

        void ImplementModelShardInterface(ModelShard modelShard)
        {
            foreach (var collection in modelShard.Collections)
            {
                Code.WriteLine($"public {DefineProperty($"ILazy{collection.Type}", collection.Name, "get; init;")} = null!;");
            }
            Code.EmptyLine();

            foreach (var relation in modelShard.Relations)
            {
                Code.WriteLine($"public {DefineProperty($"ILazy{relation.Type}", relation.Name, "get; init;")} = null!;");
            }
        }

        void ImplementSaveMethod(ModelShard modelShard)
        {
            Code.WriteLine("public void Save(IRepository repository)");
            Code.Block(() =>
            {
            });
        }
    }

    protected override void DefineMutableModelShardClass(ModelShard modelShard)
    {
        var visibility = GetInternalTypeVisibility(modelShard);

        Code.GeneratedClassAttributes(modelShard.Scheme.Debug);
        Code.Class(visibility, "sealed", $"Mutable{modelShard.Name}ModelShard",
            [
                $"IMutable{modelShard.Name}ModelShard",
                $"IMutableState<I{modelShard.Name}ModelShard>"
            ],
            () =>
            {
                DefineManualLoadRequiredProperty(modelShard);
                Code.EmptyLine();
                ImplementModelShardInterface(modelShard);
                Code.EmptyLine();
                ImplementMutableStateInterface(modelShard);
                Code.EmptyLine();
                ImplementLoadMethod(modelShard);
                Code.EmptyLine();
                ImplementSaveMethod(modelShard);
            });

        void DefineManualLoadRequiredProperty(ModelShard modelShard)
        {
            Code.WriteLine($"public bool ManualLoadRequired => {modelShard.LoadManually.ToString().ToLowerInvariant()};");
        }

        void ImplementModelShardInterface(ModelShard modelShard)
        {
            foreach (var collection in modelShard.Collections)
            {
                Code.WriteLine($"public {DefineProperty($"IMutableLazy{collection.Type}", collection.Name, "get; init;")} = null!;");
            }
            Code.EmptyLine();

            foreach (var relation in modelShard.Relations)
            {
                Code.WriteLine($"public {DefineProperty($"IMutableLazy{relation.Type}", relation.Name, "get; init;")} = null!;");
            }
        }

        void ImplementMutableStateInterface(ModelShard modelShard)
        {
            Code.WriteLine($"public I{modelShard.Name}ModelShard AsReadOnly()");
            Code.Block(() =>
            {
                Code.WriteLine($"return new {modelShard.Name}ModelShard(this);");
            });
        }

        void ImplementLoadMethod(ModelShard modelShard)
        {
            Code.WriteLine("public void Load(IRepository repository, bool force = false)");
            Code.Block(() =>
            {
            });
        }

        void ImplementSaveMethod(ModelShard modelShard)
        {
            Code.WriteLine("public void Save(IRepository repository)");
            Code.Block(() =>
            {
            });
        }
    }

    protected override void DefineModelShardClassAsReadOnlyState(ModelShard modelShard)
    {
        Code.Class(modelShard.Visibility, "sealed partial", $"{modelShard.Name}ModelShard",
        [
            $"IReadOnlyState<IMutable{modelShard.Name}ModelShard>"
        ],
        () =>
        {
            Code.WriteLine("public IChangesFrame Create()");
            Code.Block(() =>
            {
                Code.WriteLine($"return new {modelShard.Name}ChangesFrame();");
            });
            Code.WriteLine();

            Code.WriteLine($"public IMutable{modelShard.Name}ModelShard AsRunCommandModel(IMutableModelChanges changes)");
            Code.Block(() =>
            {
                Code.WriteLine($"var frame = new {modelShard.Name}ChangesFrame();");
                Code.WriteLine($"changes.AddOrGet(frame);");
                Code.WriteLine();

                Code.WriteLine($"return new Mutable{modelShard.Name}ModelShard()");
                Code.Block(() =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        Code.WriteLine($"{collection.Name} = new TrackableLazy{collection.Type}(frame.{collection.Name}, (IMutableLazy{collection.Type}){collection.Name}),");
                    }
                    Code.EmptyLine();

                    foreach (var relation in modelShard.Relations)
                    {
                        Code.WriteLine($"{relation.Name} = new TrackableLazy{relation.Type}(frame.{relation.Name}, (IMutableLazy{relation.Type}){relation.Name}),");
                    }
                }, true);
            });
            Code.WriteLine();

            Code.WriteLine($"public IMutable{modelShard.Name}ModelShard AsLoadModel(IMutableModelChanges changes)");
            Code.Block(() =>
            {
                Code.WriteLine($"var frame = new {modelShard.Name}ChangesFrame();");
                Code.WriteLine($"changes.AddOrGet(frame);");
                Code.WriteLine();

                Code.WriteLine($"return new Mutable{modelShard.Name}ModelShard()");
                Code.Block(() =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        Code.WriteLine($"{collection.Name} = new TrackableLazy{collection.Type}(frame.{collection.Name}, (IMutableLazy{collection.Type}){collection.Name}),");
                    }
                    Code.EmptyLine();

                    foreach (var relation in modelShard.Relations)
                    {
                        Code.WriteLine($"{relation.Name} = new TrackableLazy{relation.Type}(frame.{relation.Name}, (IMutableLazy{relation.Type}){relation.Name}),");
                    }
                }, true);
            });
            Code.WriteLine();

            Code.WriteLine($"public IMutable{modelShard.Name}ModelShard AsApplyModel()");
            Code.Block(() =>
            {
                Code.WriteLine($"return new Mutable{modelShard.Name}ModelShard()");
                Code.Block(() =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        Code.WriteLine($"{collection.Name} = (IMutableLazy{collection.Type}){collection.Name},");
                    }
                    Code.EmptyLine();

                    foreach (var relation in modelShard.Relations)
                    {
                        Code.WriteLine($"{relation.Name} = (IMutableLazy{relation.Type}){relation.Name},");
                    }
                }, true);
            });
        });
    }

    protected override void DefineModelShardViewClass(ModelShard modelShard)
    {
    }

    protected override void DefineApplyMethod(ModelShard modelShard)
    {
        Code.WriteLine($"public async global::System.Threading.Tasks.Task ApplyAsync(IModel model, global::System.Threading.CancellationToken token)");
        Code.Block(() =>
        {
            Code.WriteLine($"var modelShard = model.Shard<IMutable{modelShard.Name}ModelShard>();");
            Code.EmptyLine();

            var colOps = modelShard.Collections.Select(x => GenerateCollectionApplyCall(x.Name));
            var relOps = modelShard.Relations.Select(x => GenerateRelationApplyCall(x.Name));

            var operations = relOps.Union(colOps);

            foreach (var op in operations)
            {
                Code.WriteLine(op);
            }
        });

        static string GenerateCollectionApplyCall(string name)
        {
            return $"await modelShard.{name}.ApplyAsync({name}, token);";
        }

        static string GenerateRelationApplyCall(string name)
        {
            return $"await modelShard.{name}.ApplyAsync({name}, token);";
        }
    }
}
