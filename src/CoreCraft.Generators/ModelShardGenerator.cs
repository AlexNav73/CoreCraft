namespace CoreCraft.Generators;

internal partial class ApplicationModelGenerator
{
    public void GenerateModelShards(IndentedTextWriter code, IEnumerable<ModelShard> shards)
    {
        foreach (var modelShard in shards)
        {
            DefineModelShardInterface(code, modelShard, false);
            code.EmptyLine();
            DefineModelShardInterface(code, modelShard, true);
            code.EmptyLine();
            DefineModelShardInfoClass(code, modelShard);
            code.EmptyLine();
            DefineModelShardClass(code, modelShard);
            code.EmptyLine();
            DefineModelShardClassAsReadOnlyState(code, modelShard);
            code.EmptyLine();
            DefineModelShardClassAsIFeatureContext(code, modelShard);
            code.EmptyLine();
            DefineChangesFrameInterface(code, modelShard);
            code.EmptyLine();
            DefineChangesFrameClass(code, modelShard);
            code.EmptyLine();
            DefineMutableModelShardClass(code, modelShard);
            code.EmptyLine();
        }
    }

    private void DefineModelShardInterface(IndentedTextWriter code, ModelShard modelShard, bool isMutable)
    {
        var mutability = isMutable ? "Mutable" : string.Empty;

        code.GeneratedInterfaceAttributes();
        code.Interface($"I{mutability}{modelShard.Name}ModelShard", [isMutable ? "IMutableModelShard" : "IModelShard"], () =>
        {
            foreach (var collection in modelShard.Collections)
            {
                code.WriteLine(DefineProperty($"I{mutability}{collection.Type}", collection.Name, "get;"));
            }

            code.EmptyLine();

            foreach (var relation in modelShard.Relations)
            {
                code.WriteLine(DefineProperty($"I{mutability}{relation.Type}", relation.Name, "get;"));
            }
        });
    }

    private void DefineModelShardInfoClass(IndentedTextWriter code, ModelShard modelShard)
    {
        var visibility = GetInternalTypeVisibility(modelShard);

        code.GeneratedClassAttributes();
        code.Class(visibility, "static", $"{modelShard.Name}ModelShardInfo",
            () =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    var properties = collection.Entity.Properties.Select(x => $"new(\"{x.Name}\", typeof({x.Type}), {x.IsNullable.ToString().ToLower()})");
                    var array = string.Join(", ", properties);

                    code.WriteLine($"public static readonly CollectionInfo {collection.Name}Info = new(\"{modelShard.Name}\", \"{collection.Name}\", new PropertyInfo[] {{ {array} }});");
                }
                code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine($"public static readonly RelationInfo {relation.Name}Info = new(\"{modelShard.Name}\", \"{relation.Name}\");");
                }
            });
    }

    private void DefineModelShardClass(IndentedTextWriter code, ModelShard modelShard)
    {
        code.GeneratedClassAttributes();
        code.Class(modelShard.Visibility, "sealed partial", $"{modelShard.Name}ModelShard", [$"I{modelShard.Name}ModelShard"], () =>
        {
            DefineCtor(code, modelShard);
            code.EmptyLine();
            DefineConversionCtor(code, modelShard);
            code.EmptyLine();
            ImplementModelShardInterface(code, modelShard);
            code.EmptyLine();
            ImplementSaveMethod(code, modelShard);
        });

        void DefineCtor(IndentedTextWriter code, ModelShard modelShard)
        {
            code.WriteLine($"public {modelShard.Name}ModelShard()");
            code.Block(() =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine($"{collection.Name} = new {collection.Type}(");
                    code.WithIndent(c =>
                    {
                        c.WriteLine($"{modelShard.Name}ModelShardInfo.{collection.Name}Info,");
                        c.WriteLine($"static id => new {collection.Entity.Name}(id),");
                        c.WriteLine($"static () => new {collection.Entity.PropertiesType}());");
                    });
                }
                code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine($"{relation.Name} = new {relation.Type}(");
                    code.WithIndent(c =>
                    {
                        c.WriteLine($"{modelShard.Name}ModelShardInfo.{relation.Name}Info,");
                        code.WriteLine($"new {relation.ParentRelationType}<{relation.Parent.Entity.Name}, {relation.Child.Entity.Name}>(),");
                        code.WriteLine($"new {relation.ChildRelationType}<{relation.Child.Entity.Name}, {relation.Parent.Entity.Name}>());");
                    });
                }
            });
        }

        void DefineConversionCtor(IndentedTextWriter code, ModelShard modelShard)
        {
            code.WriteLine($"internal {modelShard.Name}ModelShard(IMutable{modelShard.Name}ModelShard mutable)");
            code.Block(() =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine($"{collection.Name} = ((IMutableState<I{collection.Type}>)mutable.{collection.Name}).AsReadOnly();");
                }
                code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine($"{relation.Name} = ((IMutableState<I{relation.Type}>)mutable.{relation.Name}).AsReadOnly();");
                }
            });
        }

        void ImplementModelShardInterface(IndentedTextWriter code, ModelShard modelShard)
        {
            foreach (var collection in modelShard.Collections)
            {
                code.WriteLine($"public {DefineProperty($"I{collection.Type}", collection.Name, "get; init;")} = null!;");
            }
            code.EmptyLine();

            foreach (var relation in modelShard.Relations)
            {
                code.WriteLine($"public {DefineProperty($"I{relation.Type}", relation.Name, "get; init;")} = null!;");
            }
        }

        void ImplementSaveMethod(IndentedTextWriter code, ModelShard modelShard)
        {
            code.WriteLine("public void Save(IRepository repository)");
            code.Block(() =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine($"{collection.Name}.Save(repository);");
                }
                code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine($"{relation.Name}.Save(repository);");
                }
            });
        }
    }

    private void DefineModelShardClassAsReadOnlyState(IndentedTextWriter code, ModelShard modelShard)
    {
        code.Class(modelShard.Visibility, "sealed partial", $"{modelShard.Name}ModelShard",
        [
            $"IReadOnlyState<IMutable{modelShard.Name}ModelShard>"
        ],
        () =>
        {
            code.WriteLine($"public IMutable{modelShard.Name}ModelShard AsMutable(global::System.Collections.Generic.IEnumerable<IFeature> features)");
            code.Block(() =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine($"var {ToCamelCase(collection.Name)} = (I{collection.MutableType}){collection.Name};");
                }
                code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine($"var {ToCamelCase(relation.Name)} = (I{relation.MutableType}){relation.Name};");
                }
                code.EmptyLine();

                code.WriteLine("foreach (var feature in features)");
                code.Block(() =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        code.WriteLine($"{ToCamelCase(collection.Name)} = feature.Decorate(this, {ToCamelCase(collection.Name)});");
                    }
                    code.EmptyLine();

                    foreach (var relation in modelShard.Relations)
                    {
                        code.WriteLine($"{ToCamelCase(relation.Name)} = feature.Decorate(this, {ToCamelCase(relation.Name)});");
                    }
                });
                code.EmptyLine();

                code.WriteLine($"return new Mutable{modelShard.Name}ModelShard()");
                code.Block(() =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        code.WriteLine($"{collection.Name} = {ToCamelCase(collection.Name)},");
                    }
                    code.EmptyLine();

                    foreach (var relation in modelShard.Relations)
                    {
                        code.WriteLine($"{relation.Name} = {ToCamelCase(relation.Name)},");
                    }
                }, true);
            });
        });
    }

    private void DefineModelShardClassAsIFeatureContext(IndentedTextWriter code, ModelShard modelShard)
    {
        code.Class(modelShard.Visibility, "sealed partial", $"{modelShard.Name}ModelShard", ["IFeatureContext"],
        () =>
        {
            code.WriteLine("IChangesFrame IFeatureContext.GetOrAddFrame(IMutableModelChanges modelChanges)");
            code.Block(() =>
            {
                code.WriteLine($"return modelChanges.Register(static () => new {modelShard.Name}ChangesFrame());");
            });
        });
    }

    private void DefineChangesFrameInterface(IndentedTextWriter code, ModelShard modelShard)
    {
        code.GeneratedInterfaceAttributes();
        code.Interface($"I{modelShard.Name}ChangesFrame", ["IChangesFrame"], () =>
        {
            foreach (var collection in modelShard.Collections)
            {
                code.WriteLine(DefineProperty($"I{collection.ChangesType}", collection.Name, "get;"));
            }

            code.EmptyLine();

            foreach (var relation in modelShard.Relations)
            {
                code.WriteLine(DefineProperty($"I{relation.ChangesType}", relation.Name, "get;"));
            }
        });
    }

    private void DefineChangesFrameClass(IndentedTextWriter code, ModelShard modelShard)
    {
        var visibility = GetInternalTypeVisibility(modelShard);

        code.GeneratedClassAttributes();
        code.Class(visibility, "sealed", $"{modelShard.Name}ChangesFrame",
            [
                $"I{modelShard.Name}ChangesFrame", "IChangesFrameEx"
            ],
            () =>
            {
                DefineCtor(code, modelShard);
                code.EmptyLine();
                ImplementModelShardChangesFrameInterface(code, modelShard);
                code.EmptyLine();
                DefineGetMethod(code, modelShard);
                code.EmptyLine();
                DefineInvertMethod(code, modelShard);
                code.EmptyLine();
                DefineApplyMethod(code, modelShard);
                code.EmptyLine();
                ImplementChangesFrameInterface(code, modelShard);
                code.EmptyLine();
                DefineMergeMethod(code, modelShard);
                code.EmptyLine();
                ImplementSaveMethod(code, modelShard);
                code.EmptyLine();
            });

        void DefineCtor(IndentedTextWriter code, ModelShard modelShard)
        {
            code.WriteLine($"public {modelShard.Name}ChangesFrame()");
            code.Block(() =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine($"{collection.Name} = new {collection.ChangesType}({modelShard.Name}ModelShardInfo.{collection.Name}Info);");
                }
                code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine($"{relation.Name} = new {relation.ChangesType}({modelShard.Name}ModelShardInfo.{relation.Name}Info);");
                }
            });
        }

        void ImplementModelShardChangesFrameInterface(IndentedTextWriter code, ModelShard modelShard)
        {
            foreach (var collection in modelShard.Collections)
            {
                code.WriteLine($"public {DefineProperty($"I{collection.ChangesType}", collection.Name)}");
            }
            code.EmptyLine();

            foreach (var relation in modelShard.Relations)
            {
                code.WriteLine($"public {DefineProperty($"I{relation.ChangesType}", relation.Name)}");
            }
        }

        void DefineGetMethod(IndentedTextWriter code, ModelShard modelShard)
        {
            code.WriteLine("ICollectionChangeSet<TEntity, TProperty>? IChangesFrame.Get<TEntity, TProperty>(ICollection<TEntity, TProperty> collection)");
            code.Block(() =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine($"if ({collection.Name}.Info == collection.Info) return {collection.Name} as ICollectionChangeSet<TEntity, TProperty>;");
                }
                code.EmptyLine();

                code.WriteLine("throw new System.InvalidOperationException(\"Unable to find collection's changes set\");");
            });
            code.EmptyLine();

            code.WriteLine("IRelationChangeSet<TParent, TChild>? IChangesFrame.Get<TParent, TChild>(IRelation<TParent, TChild> relation)");
            code.Block(() =>
            {
                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine($"if ({relation.Name}.Info == relation.Info) return {relation.Name} as IRelationChangeSet<TParent, TChild>;");
                }
                code.EmptyLine();

                code.WriteLine("throw new System.InvalidOperationException($\"Unable to find relation's change set\");");
            });
        }

        void DefineInvertMethod(IndentedTextWriter code, ModelShard modelShard)
        {
            code.WriteLine($"IChangesFrame IChangesFrame.Invert()");
            code.Block(() =>
            {
                code.WriteLine($"return new {modelShard.Name}ChangesFrame()");
                code.Block(() =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        code.WriteLine($"{collection.Name} = {collection.Name}.Invert(),");
                    }
                    code.EmptyLine();

                    foreach (var relation in modelShard.Relations)
                    {
                        code.WriteLine($"{relation.Name} = {relation.Name}.Invert(),");
                    }
                }, true);
            });
        }

        void DefineApplyMethod(IndentedTextWriter code, ModelShard modelShard)
        {
            code.WriteLine($"public void Apply(IModel model)");
            code.Block(() =>
            {
                code.WriteLine($"var modelShard = model.Shard<IMutable{modelShard.Name}ModelShard>();");
                code.EmptyLine();

                var operations = modelShard.Relations.Select(x => $"{x.Name}.Apply(modelShard.{x.Name});")
                    .Union(modelShard.Collections.Select(x => $"{x.Name}.Apply(modelShard.{x.Name});"));

                foreach (var op in operations)
                {
                    code.WriteLine(op);
                }
            });
        }

        void ImplementChangesFrameInterface(IndentedTextWriter code, ModelShard modelShard)
        {
            code.WriteLine($"public bool HasChanges()");
            code.Block(() =>
            {
                var checks = modelShard.Collections.Select(x => $"{x.Name}.HasChanges()")
                    .Union(modelShard.Relations.Select(x => $"{x.Name}.HasChanges()"));

                code.WriteLine($"return {string.Join(" || ", checks)};");
            });
        }

        void DefineMergeMethod(IndentedTextWriter code, ModelShard modelShard)
        {
            code.WriteLine($"public IChangesFrame Merge(IChangesFrame frame)");
            code.Block(() =>
            {
                code.WriteLine($"var typedFrame = ({modelShard.Name}ChangesFrame)frame;");
                code.EmptyLine();

                code.WriteLine($"return new {modelShard.Name}ChangesFrame()");
                code.Block(() =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        code.WriteLine($"{collection.Name} = {collection.Name}.Merge(typedFrame.{collection.Name}),");
                    }
                    code.EmptyLine();

                    foreach (var relation in modelShard.Relations)
                    {
                        code.WriteLine($"{relation.Name} = {relation.Name}.Merge(typedFrame.{relation.Name}),");
                    }
                }, true);
            });
        }

        void ImplementSaveMethod(IndentedTextWriter code, ModelShard modelShard)
        {
            code.WriteLine($"public void Save(IRepository repository)");
            code.Block(() =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine($"repository.Save({collection.Name});");
                }
                code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine($"repository.Save({relation.Name});");
                }
            });
        }
    }

    private void DefineMutableModelShardClass(IndentedTextWriter code, ModelShard modelShard)
    {
        var visibility = GetInternalTypeVisibility(modelShard);

        code.GeneratedClassAttributes();
        code.Class(visibility, "sealed", $"Mutable{modelShard.Name}ModelShard",
            [
                $"IMutable{modelShard.Name}ModelShard",
                $"IMutableState<I{modelShard.Name}ModelShard>"
            ],
            () =>
            {
                DefineManualLoadRequiredProperty(code, modelShard);
                code.EmptyLine();
                ImplementModelShardInterface(code, modelShard);
                code.EmptyLine();
                ImplementMutableStateInterface(code, modelShard);
                code.EmptyLine();
                ImplementLoadMethod(code, modelShard);
                code.EmptyLine();
                ImplementSaveMethod(code, modelShard);
            });

        void DefineManualLoadRequiredProperty(IndentedTextWriter code, ModelShard modelShard)
        {
            code.WriteLine($"public bool ManualLoadRequired => {modelShard.LoadManually.ToString().ToLowerInvariant()};");
        }

        void ImplementModelShardInterface(IndentedTextWriter code, ModelShard modelShard)
        {
            foreach (var collection in modelShard.Collections)
            {
                code.WriteLine($"public {DefineProperty($"I{collection.MutableType}", collection.Name, "get; init;")} = null!;");
            }
            code.EmptyLine();

            foreach (var relation in modelShard.Relations)
            {
                code.WriteLine($"public {DefineProperty($"I{relation.MutableType}", relation.Name, "get; init;")} = null!;");
            }
        }

        void ImplementMutableStateInterface(IndentedTextWriter code, ModelShard modelShard)
        {
            code.WriteLine($"public I{modelShard.Name}ModelShard AsReadOnly()");
            code.Block(() =>
            {
                code.WriteLine($"return new {modelShard.Name}ModelShard(this);");
            });
        }

        void ImplementLoadMethod(IndentedTextWriter code, ModelShard modelShard)
        {
            code.WriteLine("public void Load(IRepository repository, bool force = false)");
            code.Block(() =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    if (collection.LoadManually)
                    {
                        code.WriteLine($"if (force) {collection.Name}.Load(repository);");
                    }
                    else
                    {
                        code.WriteLine($"{collection.Name}.Load(repository);");
                    }
                }
                code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    if (relation.Parent.LoadManually || relation.Child.LoadManually)
                    {
                        code.WriteLine($"if (force) {relation.Name}.Load(repository, {relation.Parent.Name}, {relation.Child.Name});");
                    }
                    else
                    {
                        code.WriteLine($"{relation.Name}.Load(repository, {relation.Parent.Name}, {relation.Child.Name});");
                    }
                }
            });
        }

        void ImplementSaveMethod(IndentedTextWriter code, ModelShard modelShard)
        {
            code.WriteLine("public void Save(IRepository repository)");
            code.Block(() =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine($"{collection.Name}.Save(repository);");
                }
                code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine($"{relation.Name}.Save(repository);");
                }
            });
        }
    }

    private static string GetInternalTypeVisibility(ModelShard modelShard)
    {
        return modelShard.Visibility switch
        {
            Visibility.All => "public",
            _ => "internal"
        };
    }
}
