namespace Navitski.Crystalized.Model.Generators;

internal partial class ApplicationModelGenerator
{
    public void GenerateModelShards(IndentedTextWriter code, IEnumerable<ModelShard> shards, string idBase)
    {
        foreach (var modelShard in shards)
        {
            DefineModelShardInterface(code, modelShard, false);
            code.EmptyLine();
            DefineModelShardInterface(code, modelShard, true);
            code.EmptyLine();
            DefineModelShardClass(code, modelShard, idBase);
            code.EmptyLine();
            DefineModelShardClassAsCanBeMutable(code, modelShard);
            code.EmptyLine();
            DefineModelShardClassAsIFeatureContext(code, modelShard);
            code.EmptyLine();
            DefineChangesFrameInterface(code, modelShard);
            code.EmptyLine();
            DefineChangesFrameClass(code, modelShard, idBase);
            code.EmptyLine();
            DefineMutableModelShardClass(code, modelShard);
            code.EmptyLine();
        }
    }

    private void DefineModelShardInterface(IndentedTextWriter code, ModelShard modelShard, bool isMutable)
    {
        var mutability = isMutable ? "Mutable" : string.Empty;

        code.GeneratedInterfaceAttributes();
        code.Interface(modelShard.Visibility, $"I{mutability}{modelShard.Name}ModelShard", new[] { "IModelShard" }, () =>
        {
            foreach (var collection in modelShard.Collections)
            {
                code.WriteLine(Property($"I{mutability}{Type(collection)}", collection.Name, "get;"));
            }

            code.EmptyLine();

            foreach (var relation in modelShard.Relations)
            {
                code.WriteLine(Property($"I{mutability}{Type(relation)}", relation.Name, "get;"));
            }
        });
    }

    private void DefineModelShardClass(IndentedTextWriter code, ModelShard modelShard, string idBase)
    {
        code.GeneratedClassAttributes();
        code.Class("sealed partial", $"{modelShard.Name}ModelShard", new[] { $"I{modelShard.Name}ModelShard" }, () =>
        {
            DefineCtor(code, modelShard, idBase);
            code.EmptyLine();
            DefineConversionCtor(code, modelShard);
            code.EmptyLine();
            ImplementModelShardInterface(code, modelShard);
        });

        void DefineCtor(IndentedTextWriter code, ModelShard modelShard, string idBase)
        {
            code.SetsRequiredMembersAttribute();
            code.WriteLine($"public {modelShard.Name}ModelShard()");
            code.Block(() =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine($"{collection.Name} = new {Type(collection)}(");
                    code.WithIndent(c =>
                    {
                        c.WriteLine($"\"{idBase}.{modelShard.Name}.{collection.Name}\",");
                        c.WriteLine($"static id => new {collection.EntityType}(id),");
                        c.WriteLine($"static () => new {collection.EntityType}Properties());");
                    });
                }
                code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine($"{relation.Name} = new {Type(relation)}(");
                    code.WithIndent(c =>
                    {
                        c.WriteLine($"\"{idBase}.{modelShard.Name}.{relation.Name}\",");
                        code.WriteLine($"new {relation.ParentRelationType}<{relation.ParentType}, {relation.ChildType}>(),");
                        code.WriteLine($"new {relation.ChildRelationType}<{relation.ChildType}, {relation.ParentType}>());");
                    });
                }
            });
        }

        void DefineConversionCtor(IndentedTextWriter code, ModelShard modelShard)
        {
            code.SetsRequiredMembersAttribute();
            code.WriteLine($"internal {modelShard.Name}ModelShard(IMutable{modelShard.Name}ModelShard mutable)");
            code.Block(() =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine($"{collection.Name} = ((ICanBeReadOnly<I{Type(collection)}>)mutable.{collection.Name}).AsReadOnly();");
                }
                code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine($"{relation.Name} = ((ICanBeReadOnly<I{Type(relation)}>)mutable.{relation.Name}).AsReadOnly();");
                }
            });
        }

        void ImplementModelShardInterface(IndentedTextWriter code, ModelShard modelShard)
        {
            foreach (var collection in modelShard.Collections)
            {
                code.WriteLine($"public required {Property($"I{Type(collection)}", collection.Name, "get; init;")}");
            }
            code.EmptyLine();

            foreach (var relation in modelShard.Relations)
            {
                code.WriteLine($"public required {Property($"I{Type(relation)}", relation.Name, "get; init;")}");
            }
        }
    }

    private void DefineModelShardClassAsCanBeMutable(IndentedTextWriter code, ModelShard modelShard)
    {
        code.Class("sealed partial", $"{modelShard.Name}ModelShard", new[]
        {
            $"ICanBeMutable<IMutable{modelShard.Name}ModelShard>"
        },
        () =>
        {
            code.WriteLine($"public IMutable{modelShard.Name}ModelShard AsMutable(global::System.Collections.Generic.IEnumerable<IFeature> features)");
            code.Block(() =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine($"var {ToCamelCase(collection.Name)} = (IMutable{Type(collection)}){collection.Name};");
                }
                code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine($"var {ToCamelCase(relation.Name)} = (IMutable{Type(relation)}){relation.Name};");
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
        code.Class("sealed partial", $"{modelShard.Name}ModelShard", new[]
        {
            $"IFeatureContext"
        },
        () =>
        {
            code.WriteLine("public IChangesFrame GetOrAddFrame(IWritableModelChanges modelChanges)");
            code.Block(() =>
            {
                code.WriteLine($"return modelChanges.Register(static () => new {modelShard.Name}ChangesFrame());");
            });
        });
    }

    private void DefineChangesFrameInterface(IndentedTextWriter code, ModelShard modelShard)
    {
        code.GeneratedInterfaceAttributes();
        code.Interface(modelShard.Visibility, $"I{modelShard.Name}ChangesFrame", new[] { "IChangesFrame" }, () =>
        {
            foreach (var collection in modelShard.Collections)
            {
                code.WriteLine(Property($"I{ChangesType(collection)}", collection.Name, "get;"));
            }

            code.EmptyLine();

            foreach (var relation in modelShard.Relations)
            {
                code.WriteLine(Property($"I{ChangesType(relation)}", relation.Name, "get;"));
            }
        });
    }

    private void DefineChangesFrameClass(IndentedTextWriter code, ModelShard modelShard, string idBase)
    {
        code.GeneratedClassAttributes();
        code.Class("sealed", $"{modelShard.Name}ChangesFrame",
            new[]
            {
                $"I{modelShard.Name}ChangesFrame",
                "IWritableChangesFrame"
            },
            () =>
            {
                DefineCtor(code, modelShard, idBase);
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
            });

        void DefineCtor(IndentedTextWriter code, ModelShard modelShard, string idBase)
        {
            code.WriteLine($"public {modelShard.Name}ChangesFrame()");
            code.Block(() =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine($"{collection.Name} = new {ChangesType(collection)}(\"{idBase}.{modelShard.Name}.{collection.Name}\");");
                }
                code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine($"{relation.Name} = new {ChangesType(relation)}(\"{idBase}.{modelShard.Name}.{relation.Name}\");");
                }
            });
        }

        void ImplementModelShardChangesFrameInterface(IndentedTextWriter code, ModelShard modelShard)
        {
            foreach (var collection in modelShard.Collections)
            {
                code.WriteLine($"public {Property($"I{ChangesType(collection)}", collection.Name)}");
            }
            code.EmptyLine();

            foreach (var relation in modelShard.Relations)
            {
                code.WriteLine($"public {Property($"I{ChangesType(relation)}", relation.Name)}");
            }
        }

        void DefineGetMethod(IndentedTextWriter code, ModelShard modelShard)
        {
            code.WriteLine("public ICollectionChangeSet<TEntity, TProperty> Get<TEntity, TProperty>(ICollection<TEntity, TProperty> collection)");
            code.WithIndent(c =>
            {
                code.WriteLine("where TEntity : Entity");
                code.WriteLine("where TProperty : Properties");
            });
            code.Block(() =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine($"if ({collection.Name}.Id == collection.Id) return {collection.Name} as ICollectionChangeSet<TEntity, TProperty>;");
                }
                code.EmptyLine();

                code.WriteLine("throw new System.InvalidOperationException(\"Unable to find collection's changes set\");");
            });
            code.EmptyLine();

            code.WriteLine("public IRelationChangeSet<TParent, TChild> Get<TParent, TChild>(IRelation<TParent, TChild> relation)");
            code.WithIndent(c =>
            {
                code.WriteLine("where TParent : Entity");
                code.WriteLine("where TChild : Entity");
            });
            code.Block(() =>
            {
                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine($"if ({relation.Name}.Id == relation.Id) return {relation.Name} as IRelationChangeSet<TParent, TChild>;");
                }
                code.EmptyLine();

                code.WriteLine("throw new System.InvalidOperationException($\"Unable to find relation's change set\");");
            });
        }

        void DefineInvertMethod(IndentedTextWriter code, ModelShard modelShard)
        {
            code.WriteLine($"public IWritableChangesFrame Invert()");
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
            code.WriteLine($"public IWritableChangesFrame Merge(IChangesFrame frame)");
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
    }

    private void DefineMutableModelShardClass(IndentedTextWriter code, ModelShard modelShard)
    {
        code.GeneratedClassAttributes();
        code.Class("sealed", $"Mutable{modelShard.Name}ModelShard",
            new[]
            {
                $"IMutable{modelShard.Name}ModelShard",
                $"ICanBeReadOnly<I{modelShard.Name}ModelShard>"
            },
            () =>
            {
                ImplementModelShardInterface(code, modelShard);
                code.EmptyLine();
                ImplementCanBeReadOnlyInterface(code, modelShard);
            });

        void ImplementModelShardInterface(IndentedTextWriter code, ModelShard modelShard)
        {
            foreach (var collection in modelShard.Collections)
            {
                code.WriteLine($"public required {Property($"IMutable{Type(collection)}", collection.Name, "get; init;")}");
            }
            code.EmptyLine();

            foreach (var relation in modelShard.Relations)
            {
                code.WriteLine($"public required {Property($"IMutable{Type(relation)}", relation.Name, "get; init;")}");
            }
        }

        void ImplementCanBeReadOnlyInterface(IndentedTextWriter code, ModelShard modelShard)
        {
            code.WriteLine($"public I{modelShard.Name}ModelShard AsReadOnly()");
            code.Block(() =>
            {
                code.WriteLine($"return new {modelShard.Name}ModelShard(this);");
            });
        }
    }
}
