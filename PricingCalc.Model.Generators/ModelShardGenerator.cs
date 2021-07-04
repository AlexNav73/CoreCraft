using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace PricingCalc.Model.Generators
{
    internal partial class ApplicationModelGenerator
    {
        public void GenerateModelShards(IndentedTextWriter code, IEnumerable<ModelShard> shards, string type)
        {
            foreach (var modelShard in shards)
            {
                DefineModelShardInterface(code, modelShard, type);
                EmptyLine(code);
                DefineModelShardClass(code, modelShard);
                EmptyLine(code);
                DefineChangesFrameInterface(code, modelShard);
                EmptyLine(code);
                DefineChangesFrameClass(code, modelShard);
                EmptyLine(code);
                DefineTrackableModelShardClass(code, modelShard);
                EmptyLine(code);
            }
        }

        private void DefineModelShardInterface(IndentedTextWriter code, ModelShard modelShard, string type)
        {
            GeneratedCodeAttribute(code);
            Interface(code, modelShard.IsInternal, $"I{modelShard.Name}ModelShard", new[] { type }, () =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine(Property($"I{CollectionType(collection)}", collection.Name, "get;"));
                }

                EmptyLine(code);

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine(Property($"I{RelationType(relation)}", relation.Name, "get;"));
                }
            });
        }

        private void DefineModelShardClass(IndentedTextWriter code, ModelShard modelShard)
        {
            GeneratedCodeAttribute(code);
            Class(code, "sealed", $"{modelShard.Name}ModelShard", new[]
            {
                $"I{modelShard.Name}ModelShard",
                $"ICopy<I{modelShard.Name}ModelShard>",
                "ITrackableModelShard",
                "IHaveStorage"
            },
            () =>
            {
                DefineCtor(code, modelShard);
                EmptyLine(code);
                ImplementModelShardInterface(code, modelShard);
                EmptyLine(code);
                ImplementIHaveStorageInterface(code);
                EmptyLine(code);
                ImplementTrackableInterface(code, modelShard);
                EmptyLine(code);
                ImplementCopyInterface(code, modelShard);
            });

            void DefineCtor(IndentedTextWriter code, ModelShard modelShard)
            {
                code.WriteLine($"public {modelShard.Name}ModelShard()");
                Block(code, () =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        code.WriteLine($"{collection.Name} = new {CollectionType(collection)}(id => new {collection.Type}(id), () => new {collection.Type}Properties());");
                    }
                    EmptyLine(code);

                    foreach (var relation in modelShard.Relations)
                    {
                        code.WriteLine($"{relation.Name} = new {RelationType(relation)}(new {relation.ParentRelationType}<I{relation.ParentType}, I{relation.ChildType}>(), new {relation.ChildRelationType}<I{relation.ChildType}, I{relation.ParentType}>());");
                    }
                    EmptyLine(code);

                    code.WriteLine($"Storage = new {modelShard.Name}ModelShardStorage(this);");
                });
            }

            void ImplementModelShardInterface(IndentedTextWriter code, ModelShard modelShard)
            {
                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine($"public {Property($"I{CollectionType(collection)}", collection.Name, "get; private set;")}");
                }
                EmptyLine(code);

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine($"public {Property($"I{RelationType(relation)}", relation.Name, "get; private set;")}");
                }
                EmptyLine(code);
            }

            void ImplementIHaveStorageInterface(IndentedTextWriter code)
            {
                code.WriteLine("public IModelShardStorage Storage { get; }");
            }

            void ImplementTrackableInterface(IndentedTextWriter code, ModelShard modelShard)
            {
                code.WriteLine($"public IModelShard AsTrackable(IWritableModelChanges modelChanges)");
                Block(code, () =>
                {
                    code.WriteLine($"var frame = modelChanges.Add(new {modelShard.Name}ChangesFrame());");
                    code.WriteLine($"return new Trackable{modelShard.Name}ModelShard(this, frame);");
                });
            }

            void ImplementCopyInterface(IndentedTextWriter code, ModelShard modelShard)
            {
                code.WriteLine($"public I{modelShard.Name}ModelShard Copy()");
                Block(code, () =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        code.WriteLine($"var {collection.Name.ToLower()} = {collection.Name}.Copy();");
                    }
                    EmptyLine(code);

                    foreach (var relation in modelShard.Relations)
                    {
                        code.WriteLine($"var {relation.Name.ToLower()} = {relation.Name}.Copy();");
                    }
                    EmptyLine(code);

                    code.WriteLine($"return new {modelShard.Name}ModelShard()");
                    Block(code, () =>
                    {
                        foreach (var collection in modelShard.Collections)
                        {
                            code.WriteLine($"{collection.Name} = {collection.Name.ToLower()},");
                        }
                        EmptyLine(code);

                        foreach (var relation in modelShard.Relations)
                        {
                            code.WriteLine($"{relation.Name} = {relation.Name.ToLower()},");
                        }
                    }, true);
                });
            }
        }

        private void DefineChangesFrameInterface(IndentedTextWriter code, ModelShard modelShard)
        {
            GeneratedCodeAttribute(code);
            Interface(code, modelShard.IsInternal, $"I{modelShard.Name}ChangesFrame", new[] { "IChangesFrame" }, () =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine(Property($"I{CollectionChangesType(collection)}", collection.Name, "get;"));
                }

                EmptyLine(code);

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine(Property($"I{RelationChangesType(relation)}", relation.Name, "get;"));
                }
            });
        }

        private void DefineChangesFrameClass(IndentedTextWriter code, ModelShard modelShard)
        {
            GeneratedCodeAttribute(code);
            Class(code, "sealed", $"{modelShard.Name}ChangesFrame",
                new[]
                {
                    $"I{modelShard.Name}ChangesFrame",
                    "IWritableChangesFrame"
                },
                () =>
                {
                    DefineCtor(code, modelShard);
                    EmptyLine(code);
                    ImplementModelShardChangesFrameInterface(code, modelShard);
                    EmptyLine(code);
                    DefineInvertMethod(code, modelShard);
                    EmptyLine(code);
                    DefineApplyMethod(code, modelShard);
                    EmptyLine(code);
                    ImplementChangesFrameInterface(code, modelShard);
                });

            void DefineCtor(IndentedTextWriter code, ModelShard modelShard)
            {
                code.WriteLine($"public {modelShard.Name}ChangesFrame()");
                Block(code, () =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        code.WriteLine($"{collection.Name} = new {CollectionChangesType(collection)}();");
                    }
                    EmptyLine(code);

                    foreach (var relation in modelShard.Relations)
                    {
                        code.WriteLine($"{relation.Name} = new {RelationChangesType(relation)}();");
                    }
                });
            }

            void ImplementModelShardChangesFrameInterface(IndentedTextWriter code, ModelShard modelShard)
            {
                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine($"public {Property($"I{CollectionChangesType(collection)}", collection.Name, "get; private set;")}");
                }
                EmptyLine(code);

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine($"public {Property($"I{RelationChangesType(relation)}", relation.Name, "get; private set;")}");
                }
            }

            void DefineInvertMethod(IndentedTextWriter code, ModelShard modelShard)
            {
                code.WriteLine($"public IWritableChangesFrame Invert()");
                Block(code, () =>
                {
                    code.WriteLine($"return new {modelShard.Name}ChangesFrame()");
                    Block(code, () =>
                    {
                        foreach (var collection in modelShard.Collections)
                        {
                            code.WriteLine($"{collection.Name} = {collection.Name}.Invert(),");
                        }
                        EmptyLine(code);

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
                Block(code, () =>
                {
                    code.WriteLine($"var modelShard = model.Shard<I{modelShard.Name}ModelShard>();");
                    EmptyLine(code);

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
                Block(code, () =>
                {
                    var checks = modelShard.Collections.Select(x => $"{x.Name}.HasChanges()")
                        .Union(modelShard.Relations.Select(x => $"{x.Name}.HasChanges()"));

                    code.WriteLine($"return {string.Join(" || ", checks)};");
                });
            }
        }

        private void DefineTrackableModelShardClass(IndentedTextWriter code, ModelShard modelShard)
        {
            GeneratedCodeAttribute(code);
            Class(code, "sealed", $"Trackable{modelShard.Name}ModelShard",
                new[]
                {
                    $"I{modelShard.Name}ModelShard",
                    "IWritableModelShard",
                    "IHaveStorage"
                },
                () =>
                {
                    DefineCtor(code, modelShard);
                    EmptyLine(code);
                    ImplementModelShardInterface(code, modelShard);
                    ImplementIHaveStorageInterface(code);
                });

            void DefineCtor(IndentedTextWriter code, ModelShard modelShard)
            {
                code.WriteLine($"public Trackable{modelShard.Name}ModelShard(I{modelShard.Name}ModelShard modelShard, I{modelShard.Name}ChangesFrame frame)");
                Block(code, () =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        code.WriteLine($"{collection.Name} = new Trackable{CollectionType(collection)}(frame.{collection.Name}, modelShard.{collection.Name});");
                    }
                    EmptyLine(code);

                    foreach (var relation in modelShard.Relations)
                    {
                        code.WriteLine($"{relation.Name} = new Trackable{RelationType(relation)}(frame.{relation.Name}, modelShard.{relation.Name});");
                    }

                    code.WriteLine($"Storage = new {modelShard.Name}ModelShardStorage(this);");
                });
            }

            void ImplementModelShardInterface(IndentedTextWriter code, ModelShard modelShard)
            {
                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine($"public {Property($"I{CollectionType(collection)}", collection.Name, "get; private set;")}");
                }
                EmptyLine(code);

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine($"public {Property($"I{RelationType(relation)}", relation.Name, "get; private set;")}");
                }
                EmptyLine(code);
            }

            void ImplementIHaveStorageInterface(IndentedTextWriter code)
            {
                code.WriteLine("public IModelShardStorage Storage { get; }");
            }
        }
    }
}
