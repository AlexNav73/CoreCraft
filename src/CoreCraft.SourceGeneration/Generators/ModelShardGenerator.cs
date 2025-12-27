using CoreCraft.SourceGeneration.Extensions;

namespace CoreCraft.SourceGeneration.Generators;

internal class ModelShardGenerator(IndentedTextWriter code) : GeneratorCommon
{
    protected readonly IndentedTextWriter Code = code;

    public void Generate(IEnumerable<ModelShard> shards)
    {
        foreach (var modelShard in shards.Where(ContainsFeature))
        {
            DefineModelShardInterface(modelShard, false);
            Code.EmptyLine();
            DefineModelShardInterface(modelShard, true);
            Code.EmptyLine();
            DefineModelShardInfoClass(modelShard);
            Code.EmptyLine();
            DefineModelShardClass(modelShard);
            Code.EmptyLine();
            DefineModelShardClassAsReadOnlyState(modelShard);
            Code.EmptyLine();
            DefineChangesFrameInterface(modelShard);
            Code.EmptyLine();
            DefineChangesFrameClass(modelShard);
            Code.EmptyLine();
            DefineMutableModelShardClass(modelShard);
            Code.EmptyLine();
            DefineModelShardViewClass(modelShard);
            Code.EmptyLine();
            DefineModelShardSpecificClasses(modelShard);
            Code.EmptyLine();
        }
    }

    protected virtual bool ContainsFeature(ModelShard modelShard)
    {
        return !modelShard.Features.Any();
    }

    protected virtual void DefineModelShardInterface(ModelShard modelShard, bool isMutable)
    {
        var mutability = isMutable ? "Mutable" : string.Empty;

        Code.GeneratedInterfaceAttributes();
        Code.Interface($"I{mutability}{modelShard.Name}ModelShard", [isMutable ? "IMutableModelShard" : "IModelShard"], () =>
        {
            foreach (var collection in modelShard.Collections)
            {
                Code.WriteLine(DefineProperty($"I{mutability}{collection.Type}", collection.Name, "get;"));
            }

            Code.EmptyLine();

            foreach (var relation in modelShard.Relations)
            {
                Code.WriteLine(DefineProperty($"I{mutability}{relation.Type}", relation.Name, "get;"));
            }
        });
    }

    protected virtual void DefineModelShardInfoClass(ModelShard modelShard)
    {
        var visibility = GetInternalTypeVisibility(modelShard);

        Code.GeneratedClassAttributes(modelShard.Scheme.Debug);
        Code.Class(visibility, "static", $"{modelShard.Name}ModelShardInfo",
            () =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    var properties = collection.Entity.Properties.Select(x => $"new(\"{x.Name}\", typeof({x.Type}), {x.IsNullable.ToString().ToLower()})");
                    var array = string.Join(", ", properties);

                    Code.WriteLine($"public static readonly CollectionInfo {collection.Name}Info = new(\"{modelShard.Name}\", \"{collection.Name}\", new PropertyInfo[] {{ {array} }});");
                }
                Code.EmptyLine();

                foreach (var name in modelShard.Relations.Select(x => x.Name))
                {
                    Code.WriteLine($"public static readonly RelationInfo {name}Info = new(\"{modelShard.Name}\", \"{name}\");");
                }
            });
    }

    protected virtual void DefineModelShardClass(ModelShard modelShard)
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
            Code.WriteLine($"public {modelShard.Name}ModelShard()");
            Code.Block(() =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    Code.WriteLine($"{collection.Name} = new {collection.Type}(");
                    Code.WithIndent(c =>
                    {
                        c.WriteLine($"{modelShard.Name}ModelShardInfo.{collection.Name}Info,");
                        c.WriteLine($"static id => new {collection.Entity.Name}(id),");
                        c.WriteLine($"static () => new {collection.Entity.PropertiesType}());");
                    });
                }
                Code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    Code.WriteLine($"{relation.Name} = new {relation.Type}(");
                    Code.WithIndent(c =>
                    {
                        c.WriteLine($"{modelShard.Name}ModelShardInfo.{relation.Name}Info,");
                        c.WriteLine($"new {relation.ParentRelationType}<{relation.Parent.Entity.Name}, {relation.Child.Entity.Name}>(),");
                        c.WriteLine($"new {relation.ChildRelationType}<{relation.Child.Entity.Name}, {relation.Parent.Entity.Name}>());");
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
                    Code.WriteLine($"{collection.Name} = ((IMutableState<I{collection.Type}>)mutable.{collection.Name}).AsReadOnly();");
                }
                Code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    Code.WriteLine($"{relation.Name} = ((IMutableState<I{relation.Type}>)mutable.{relation.Name}).AsReadOnly();");
                }
            });
        }

        void ImplementModelShardInterface(ModelShard modelShard)
        {
            foreach (var collection in modelShard.Collections)
            {
                Code.WriteLine($"public {DefineProperty($"I{collection.Type}", collection.Name, "get; init;")} = null!;");
            }
            Code.EmptyLine();

            foreach (var relation in modelShard.Relations)
            {
                Code.WriteLine($"public {DefineProperty($"I{relation.Type}", relation.Name, "get; init;")} = null!;");
            }
        }

        void ImplementSaveMethod(ModelShard modelShard)
        {
            Code.WriteLine("public void Save(IRepository repository)");
            Code.Block(() =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    Code.WriteLine($"{collection.Name}.Save(repository);");
                }
                Code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    Code.WriteLine($"{relation.Name}.Save(repository);");
                }
            });
        }
    }

    protected virtual void DefineModelShardClassAsReadOnlyState(ModelShard modelShard)
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
                        Code.WriteLine($"{collection.Name} = new Trackable{collection.Type}(frame.{collection.Name}, new CoW{collection.Type}({collection.Name})),");
                    }
                    Code.EmptyLine();

                    foreach (var relation in modelShard.Relations)
                    {
                        Code.WriteLine($"{relation.Name} = new Trackable{relation.Type}(frame.{relation.Name}, new CoW{relation.Type}({relation.Name})),");
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
                        Code.WriteLine($"{collection.Name} = new Trackable{collection.Type}(frame.{collection.Name}, (I{collection.MutableType}){collection.Name}),");
                    }
                    Code.EmptyLine();

                    foreach (var relation in modelShard.Relations)
                    {
                        Code.WriteLine($"{relation.Name} = new Trackable{relation.Type}(frame.{relation.Name}, (I{relation.MutableType}){relation.Name}),");
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
                        Code.WriteLine($"{collection.Name} = new CoW{collection.Type}({collection.Name}),");
                    }
                    Code.EmptyLine();

                    foreach (var relation in modelShard.Relations)
                    {
                        Code.WriteLine($"{relation.Name} = new CoW{relation.Type}({relation.Name}),");
                    }
                }, true);
            });
        });
    }

    protected virtual void DefineChangesFrameInterface(ModelShard modelShard)
    {
        Code.GeneratedInterfaceAttributes();
        Code.Interface($"I{modelShard.Name}ChangesFrame", ["IChangesFrame"], () =>
        {
            foreach (var collection in modelShard.Collections)
            {
                Code.WriteLine(DefineProperty($"I{collection.ChangesType}", collection.Name, "get;"));
            }

            Code.EmptyLine();

            foreach (var relation in modelShard.Relations)
            {
                Code.WriteLine(DefineProperty($"I{relation.ChangesType}", relation.Name, "get;"));
            }
        });
    }

    protected virtual void DefineChangesFrameClass(ModelShard modelShard)
    {
        var visibility = GetInternalTypeVisibility(modelShard);

        Code.GeneratedClassAttributes(modelShard.Scheme.Debug);
        Code.Class(visibility, "sealed", $"{modelShard.Name}ChangesFrame",
            [
                $"I{modelShard.Name}ChangesFrame", "IChangesFrameEx"
            ],
            () =>
            {
                DefineCtor(modelShard);
                Code.EmptyLine();
                ImplementModelShardChangesFrameInterface(modelShard);
                Code.EmptyLine();
                DefineInvertMethod(modelShard);
                Code.EmptyLine();
                DefineApplyMethod(modelShard);
                Code.EmptyLine();
                ImplementChangesFrameInterface(modelShard);
                Code.EmptyLine();
                DefineMergeMethod(modelShard);
                Code.EmptyLine();
                ImplementDoMethod(modelShard);
                Code.EmptyLine();
            });

        void DefineCtor(ModelShard modelShard)
        {
            Code.WriteLine($"public {modelShard.Name}ChangesFrame()");
            Code.Block(() =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    Code.WriteLine($"{collection.Name} = new {collection.ChangesType}({modelShard.Name}ModelShardInfo.{collection.Name}Info);");
                }
                Code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    Code.WriteLine($"{relation.Name} = new {relation.ChangesType}({modelShard.Name}ModelShardInfo.{relation.Name}Info);");
                }
            });
        }

        void ImplementModelShardChangesFrameInterface(ModelShard modelShard)
        {
            foreach (var collection in modelShard.Collections)
            {
                Code.WriteLine($"public {DefineProperty($"I{collection.ChangesType}", collection.Name)}");
            }
            Code.EmptyLine();

            foreach (var relation in modelShard.Relations)
            {
                Code.WriteLine($"public {DefineProperty($"I{relation.ChangesType}", relation.Name)}");
            }
        }

        void DefineInvertMethod(ModelShard modelShard)
        {
            Code.WriteLine($"public IChangesFrame Invert()");
            Code.Block(() =>
            {
                Code.WriteLine($"return new {modelShard.Name}ChangesFrame()");
                Code.Block(() =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        Code.WriteLine($"{collection.Name} = {collection.Name}.Invert(),");
                    }
                    Code.EmptyLine();

                    foreach (var relation in modelShard.Relations)
                    {
                        Code.WriteLine($"{relation.Name} = {relation.Name}.Invert(),");
                    }
                }, true);
            });
        }

        void ImplementChangesFrameInterface(ModelShard modelShard)
        {
            Code.WriteLine($"public bool HasChanges()");
            Code.Block(() =>
            {
                var checks = modelShard.Collections.Select(x => $"{x.Name}.HasChanges()")
                    .Union(modelShard.Relations.Select(x => $"{x.Name}.HasChanges()"));

                Code.WriteLine($"return {string.Join(" || ", checks)};");
            });
        }

        void DefineMergeMethod(ModelShard modelShard)
        {
            Code.WriteLine($"public IChangesFrame Merge(IChangesFrame frame)");
            Code.Block(() =>
            {
                Code.WriteLine($"var typedFrame = ({modelShard.Name}ChangesFrame)frame;");
                Code.EmptyLine();

                Code.WriteLine($"return new {modelShard.Name}ChangesFrame()");
                Code.Block(() =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        Code.WriteLine($"{collection.Name} = {collection.Name}.Merge(typedFrame.{collection.Name}),");
                    }
                    Code.EmptyLine();

                    foreach (var relation in modelShard.Relations)
                    {
                        Code.WriteLine($"{relation.Name} = {relation.Name}.Merge(typedFrame.{relation.Name}),");
                    }
                }, true);
            });
        }

        void ImplementDoMethod(ModelShard modelShard)
        {
            Code.WriteLine($"public void Do<T>(T operation)");
            Code.WithIndent(c => c.WriteLine("where T : IChangesFrameOperation"));
            Code.Block(() =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    Code.WriteLine($"operation.OnCollection({collection.Name});");
                }
                Code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    Code.WriteLine($"operation.OnRelation({relation.Name});");
                }
            });
        }
    }

    protected virtual void DefineApplyMethod(ModelShard modelShard)
    {
        Code.WriteLine($"public async global::System.Threading.Tasks.Task ApplyAsync(IModel model, global::System.Threading.CancellationToken token = default)");
        Code.Block(() =>
        {
            Code.WriteLine($"var modelShard = model.Shard<IMutable{modelShard.Name}ModelShard>();");
            Code.EmptyLine();

            var operations = modelShard.Relations.Select(x => $"await modelShard.{x.Name}.ApplyAsync({x.Name}, token);")
                .Union(modelShard.Collections.Select(x => $"await modelShard.{x.Name}.ApplyAsync({x.Name}, token);"));

            foreach (var op in operations)
            {
                Code.WriteLine(op);
            }
        });
    }

    protected virtual void DefineMutableModelShardClass(ModelShard modelShard)
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
                Code.WriteLine($"public {DefineProperty($"I{collection.MutableType}", collection.Name, "get; init;")} = null!;");
            }
            Code.EmptyLine();

            foreach (var relation in modelShard.Relations)
            {
                Code.WriteLine($"public {DefineProperty($"I{relation.MutableType}", relation.Name, "get; init;")} = null!;");
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
                foreach (var collection in modelShard.Collections)
                {
                    if (collection.LoadManually)
                    {
                        Code.WriteLine($"if (force) {collection.Name}.Load(repository);");
                    }
                    else
                    {
                        Code.WriteLine($"{collection.Name}.Load(repository);");
                    }
                }
                Code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    if (relation.Parent.LoadManually || relation.Child.LoadManually)
                    {
                        Code.WriteLine($"if (force) {relation.Name}.Load(repository, {relation.Parent.Name}, {relation.Child.Name});");
                    }
                    else
                    {
                        Code.WriteLine($"{relation.Name}.Load(repository, {relation.Parent.Name}, {relation.Child.Name});");
                    }
                }
            });
        }

        void ImplementSaveMethod(ModelShard modelShard)
        {
            Code.WriteLine("public void Save(IRepository repository)");
            Code.Block(() =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    Code.WriteLine($"{collection.Name}.Save(repository);");
                }
                Code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    Code.WriteLine($"{relation.Name}.Save(repository);");
                }
            });
        }
    }

    protected virtual void DefineModelShardViewClass(ModelShard modelShard)
    {
        var visibility = GetInternalTypeVisibility(modelShard);

        Code.GeneratedClassAttributes(modelShard.Scheme.Debug);
        Code.Class(visibility, "sealed partial", $"{modelShard.Name}ModelShardView",
            [
                "global::System.IDisposable"
            ],
            () =>
            {
                Code.WriteLine("private bool _disposed = false;");
                Code.EmptyLine();
                
                ImplementCtor(modelShard);
                Code.EmptyLine();
                ImplementModelShardInterface(modelShard);
                Code.EmptyLine();
                ImplementDisposeInterface(modelShard);
            });

        void ImplementCtor(ModelShard modelShard)
        {
            Code.WriteLine($"public {modelShard.Name}ModelShardView(IDomainModel model)");
            Code.Block(() =>
            {
                Code.WriteLine($"var builder = model.View<I{modelShard.Name}ModelShard, I{modelShard.Name}ChangesFrame>();");
                Code.EmptyLine();

                foreach (var collection in modelShard.Collections)
                {
                    Code.WriteLine($"{collection.Name} = builder.Create(static shard => shard.{collection.Name}, static frame => frame.{collection.Name});");
                }
                Code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    Code.WriteLine($"{relation.Name} = builder.Create(static shard => shard.{relation.Name}, static frame => frame.{relation.Name});");
                }
            });
        }

        void ImplementDisposeInterface(ModelShard modelShard)
        {
            Code.WriteLine("public void Dispose()");
            Code.Block(() =>
            {
                Code.WriteLine("Dispose(true);");
                Code.WriteLine("global::System.GC.SuppressFinalize(this);");
            });
            Code.EmptyLine();

            Code.WriteLine($"~{modelShard.Name}ModelShardView()");
            Code.Block(() =>
            {
                Code.WriteLine("Dispose(false);");
            });
            Code.EmptyLine();

            Code.WriteLine("private void Dispose(bool disposing)");
            Code.Block(() =>
            {
                Code.WriteLine("if (_disposed)");
                Code.Block(() =>
                {
                    Code.WriteLine("return;");
                });
                Code.EmptyLine();

                foreach (var collection in modelShard.Collections)
                {
                    Code.WriteLine($"{collection.Name}.Dispose();");
                }
                Code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    Code.WriteLine($"{relation.Name}.Dispose();");
                }
                Code.EmptyLine();

                Code.WriteLine("_disposed = true;");
            });
        }

        void ImplementModelShardInterface(ModelShard modelShard)
        {
            foreach (var collection in modelShard.Collections)
            {
                Code.WriteLine($"public {collection.ViewType} {collection.Name} {{ get; private set; }}");
            }
            Code.EmptyLine();

            foreach (var relation in modelShard.Relations)
            {
                Code.WriteLine($"public {relation.ViewType} {relation.Name} {{ get; private set; }}");
            }
            Code.EmptyLine();

            Code.WriteLine("public void Save(IRepository repository)");
            Code.Block(() =>
            {
                Code.WriteLine("throw new global::System.InvalidOperationException(\"Cannot save model shard's view. Call Save on the real model shard.\");");
            });
        }
    }

    protected virtual void DefineModelShardSpecificClasses(ModelShard modelShard)
    {
    }

    protected static string GetInternalTypeVisibility(ModelShard modelShard)
    {
        return modelShard.Visibility switch
        {
            Visibility.All => "public",
            _ => "internal"
        };
    }
}
