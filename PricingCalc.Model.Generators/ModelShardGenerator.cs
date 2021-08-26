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
                code.EmptyLine();
                DefineModelShardClass(code, modelShard);
                code.EmptyLine();
                DefineChangesFrameInterface(code, modelShard);
                code.EmptyLine();
                DefineChangesFrameClass(code, modelShard);
                code.EmptyLine();
                DefineTrackableModelShardClass(code, modelShard);
                code.EmptyLine();
            }
        }

        private void DefineModelShardInterface(IndentedTextWriter code, ModelShard modelShard, string type)
        {
            code.GeneratedCodeAttribute();
            code.Interface(modelShard.IsInternal, $"I{modelShard.Name}ModelShard", new[] { type }, () =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine(Property($"I{Type(collection)}", collection.Name, "get;"));
                }

                code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine(Property($"I{Type(relation)}", relation.Name, "get;"));
                }
            });
        }

        private void DefineModelShardClass(IndentedTextWriter code, ModelShard modelShard)
        {
            code.GeneratedCodeAttribute();
            code.Class("sealed", $"{modelShard.Name}ModelShard", new[]
            {
                $"I{modelShard.Name}ModelShard",
                $"ICopy<I{modelShard.Name}ModelShard>",
                "ITrackableModelShard",
                "IHaveStorage"
            },
            () =>
            {
                DefineCtor(code, modelShard);
                code.EmptyLine();
                ImplementModelShardInterface(code, modelShard);
                code.EmptyLine();
                ImplementIHaveStorageInterface(code);
                code.EmptyLine();
                ImplementTrackableInterface(code, modelShard);
                code.EmptyLine();
                ImplementCopyInterface(code, modelShard);
            });

            void DefineCtor(IndentedTextWriter code, ModelShard modelShard)
            {
                code.WriteLine($"public {modelShard.Name}ModelShard()");
                code.Block(() =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        code.WriteLine($"{collection.Name} = new {Type(collection)}(id => new {collection.Type}(id), () => new {collection.Type}Properties());");
                    }
                    code.EmptyLine();

                    foreach (var relation in modelShard.Relations)
                    {
                        code.WriteLine($"{relation.Name} = new {Type(relation)}(new {relation.ParentRelationType}<I{relation.ParentType}, I{relation.ChildType}>(), new {relation.ChildRelationType}<I{relation.ChildType}, I{relation.ParentType}>());");
                    }
                    code.EmptyLine();

                    code.WriteLine($"Storage = new {modelShard.Name}ModelShardStorage(this);");
                });
            }

            void ImplementModelShardInterface(IndentedTextWriter code, ModelShard modelShard)
            {
                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine($"public {Property($"I{Type(collection)}", collection.Name, "get; private set;")}");
                }
                code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine($"public {Property($"I{Type(relation)}", relation.Name, "get; private set;")}");
                }
                code.EmptyLine();
            }

            void ImplementIHaveStorageInterface(IndentedTextWriter code)
            {
                code.WriteLine("public IModelShardStorage Storage { get; }");
            }

            void ImplementTrackableInterface(IndentedTextWriter code, ModelShard modelShard)
            {
                code.WriteLine($"public IModelShard AsTrackable(IWritableModelChanges modelChanges)");
                code.Block(() =>
                {
                    code.WriteLine($"var frame = modelChanges.Add(new {modelShard.Name}ChangesFrame());");
                    code.WriteLine($"return new Trackable{modelShard.Name}ModelShard(this, frame);");
                });
            }

            void ImplementCopyInterface(IndentedTextWriter code, ModelShard modelShard)
            {
                code.WriteLine($"public I{modelShard.Name}ModelShard Copy()");
                code.Block(() =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        code.WriteLine($"var {collection.Name.ToLower()} = {collection.Name}.Copy();");
                    }
                    code.EmptyLine();

                    foreach (var relation in modelShard.Relations)
                    {
                        code.WriteLine($"var {relation.Name.ToLower()} = {relation.Name}.Copy();");
                    }
                    code.EmptyLine();

                    code.WriteLine($"return new {modelShard.Name}ModelShard()");
                    code.Block(() =>
                    {
                        foreach (var collection in modelShard.Collections)
                        {
                            code.WriteLine($"{collection.Name} = {collection.Name.ToLower()},");
                        }
                        code.EmptyLine();

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
            code.GeneratedCodeAttribute();
            code.Interface(modelShard.IsInternal, $"I{modelShard.Name}ChangesFrame", new[] { "IChangesFrame" }, () =>
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

        private void DefineChangesFrameClass(IndentedTextWriter code, ModelShard modelShard)
        {
            code.GeneratedCodeAttribute();
            code.Class("sealed", $"{modelShard.Name}ChangesFrame",
                new[]
                {
                    $"I{modelShard.Name}ChangesFrame",
                    "IWritableChangesFrame"
                },
                () =>
                {
                    DefineCtor(code, modelShard);
                    code.EmptyLine();
                    ImplementModelShardChangesFrameInterface(code, modelShard);
                    code.EmptyLine();
                    DefineInvertMethod(code, modelShard);
                    code.EmptyLine();
                    DefineApplyMethod(code, modelShard);
                    code.EmptyLine();
                    ImplementChangesFrameInterface(code, modelShard);
                });

            void DefineCtor(IndentedTextWriter code, ModelShard modelShard)
            {
                code.WriteLine($"public {modelShard.Name}ChangesFrame()");
                code.Block(() =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        code.WriteLine($"{collection.Name} = new {ChangesType(collection)}();");
                    }
                    code.EmptyLine();

                    foreach (var relation in modelShard.Relations)
                    {
                        code.WriteLine($"{relation.Name} = new {ChangesType(relation)}();");
                    }
                });
            }

            void ImplementModelShardChangesFrameInterface(IndentedTextWriter code, ModelShard modelShard)
            {
                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine($"public {Property($"I{ChangesType(collection)}", collection.Name, "get; private set;")}");
                }
                code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine($"public {Property($"I{ChangesType(relation)}", relation.Name, "get; private set;")}");
                }
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
                    code.WriteLine($"var modelShard = model.Shard<I{modelShard.Name}ModelShard>();");
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
        }

        private void DefineTrackableModelShardClass(IndentedTextWriter code, ModelShard modelShard)
        {
            code.GeneratedCodeAttribute();
            code.Class("sealed", $"Trackable{modelShard.Name}ModelShard",
                new[]
                {
                    $"I{modelShard.Name}ModelShard",
                    "IWritableModelShard",
                    "IHaveStorage"
                },
                () =>
                {
                    DefineCtor(code, modelShard);
                    code.EmptyLine();
                    ImplementModelShardInterface(code, modelShard);
                    ImplementIHaveStorageInterface(code);
                });

            void DefineCtor(IndentedTextWriter code, ModelShard modelShard)
            {
                code.WriteLine($"public Trackable{modelShard.Name}ModelShard(I{modelShard.Name}ModelShard modelShard, I{modelShard.Name}ChangesFrame frame)");
                code.Block(() =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        code.WriteLine($"{collection.Name} = new Trackable{Type(collection)}(frame.{collection.Name}, modelShard.{collection.Name});");
                    }
                    code.EmptyLine();

                    foreach (var relation in modelShard.Relations)
                    {
                        code.WriteLine($"{relation.Name} = new Trackable{Type(relation)}(frame.{relation.Name}, modelShard.{relation.Name});");
                    }

                    code.WriteLine($"Storage = new {modelShard.Name}ModelShardStorage(this);");
                });
            }

            void ImplementModelShardInterface(IndentedTextWriter code, ModelShard modelShard)
            {
                foreach (var collection in modelShard.Collections)
                {
                    code.WriteLine($"public {Property($"I{Type(collection)}", collection.Name, "get; private set;")}");
                }
                code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    code.WriteLine($"public {Property($"I{Type(relation)}", relation.Name, "get; private set;")}");
                }
                code.EmptyLine();
            }

            void ImplementIHaveStorageInterface(IndentedTextWriter code)
            {
                code.WriteLine("public IModelShardStorage Storage { get; }");
            }
        }
    }
}
