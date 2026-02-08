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
        Code.Interface($"I{mutability}{modelShard.Name}LazyModelShard", [isMutable ? "IMutableModelShard" : "ILazyModelShard"], () =>
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
        Code.Class(modelShard.Visibility, "sealed partial", $"{modelShard.Name}LazyModelShard", [$"I{modelShard.Name}LazyModelShard"], () =>
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
            Code.WriteLine($"public {modelShard.Name}LazyModelShard(DataConnection db)");
            Code.Block(() =>
            {
                foreach (var collection in modelShard.Collections)
                {
                    Code.WriteLine($"db.CreateTable<{collection.Entity.PropertiesType}>(");
                    Code.WithIndent(c =>
                    {
                        c.WriteLine($"schemaName: {modelShard.Name}ModelShardInfo.{collection.Name}Info.ShardName,");
                        c.WriteLine("tableOptions: TableOptions.CreateIfNotExists);");
                    });
                }
                Code.EmptyLine();

                foreach (var relation in modelShard.Relations)
                {
                    var parentTypeName = relation.Parent.Entity.Name;
                    var childTypeName = relation.Child.Entity.Name;
                    var parentRelationType = relation.RelationType is RelationType.OneToOne or RelationType.OneToMany ? "One" : "Many";
                    var childRelationType = relation.RelationType is RelationType.OneToOne ? "One" : "Many";
                    Code.WriteLine($"db.CreateTable<ParentToChild<{parentRelationType}, {parentTypeName}, {childRelationType}, {childTypeName}>>(");
                    Code.WithIndent(c =>
                    {
                        c.WriteLine($"schemaName: {modelShard.Name}ModelShardInfo.{relation.Name}Info.ShardName,");
                        c.WriteLine("tableOptions: TableOptions.CreateIfNotExists);");
                    });
                }
                Code.EmptyLine();

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
                    var parentTypeName = relation.Parent.Entity.Name;
                    var childTypeName = relation.Child.Entity.Name;

                    Code.WriteLine($"{relation.Name} = new LazyRelation<{parentTypeName}, {childTypeName}>(");
                    Code.WithIndent(c =>
                    {
                        c.WriteLine($"{modelShard.Name}ModelShardInfo.{relation.Name}Info,");
                        var getTableExpression = relation.RelationType switch
                        {
                            RelationType.OneToOne => $"db.GetTable<ParentToChild<One, {parentTypeName}, One, {childTypeName}>>());",
                            RelationType.OneToMany => $"db.GetTable<ParentToChild<One, {parentTypeName}, Many, {childTypeName}>>());",
                            RelationType.ManyToMany => $"db.GetTable<ParentToChild<Many, {parentTypeName}, Many, {childTypeName}>>());",
                            var t => throw new NotSupportedException($"{t} is not supported relation type")
                        };
                        Code.WriteLine(getTableExpression);
                    });
                }
            });
        }

        void DefineConversionCtor(ModelShard modelShard)
        {
            Code.WriteLine($"internal {modelShard.Name}LazyModelShard(IMutable{modelShard.Name}LazyModelShard mutable)");
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
        Code.Class(visibility, "sealed", $"Mutable{modelShard.Name}LazyModelShard",
            [
                $"IMutable{modelShard.Name}LazyModelShard",
                $"IMutableState<I{modelShard.Name}LazyModelShard>"
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
            Code.WriteLine($"public I{modelShard.Name}LazyModelShard AsReadOnly()");
            Code.Block(() =>
            {
                Code.WriteLine($"return new {modelShard.Name}LazyModelShard(this);");
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
        Code.Class(modelShard.Visibility, "sealed partial", $"{modelShard.Name}LazyModelShard",
        [
            $"IReadOnlyState<IMutable{modelShard.Name}LazyModelShard>"
        ],
        () =>
        {
            Code.WriteLine("public IChangesFrame Create()");
            Code.Block(() =>
            {
                Code.WriteLine($"return new {modelShard.Name}ChangesFrame();");
            });
            Code.WriteLine();

            Code.WriteLine($"public IMutable{modelShard.Name}LazyModelShard AsRunCommandModel(IMutableModelChanges changes)");
            Code.Block(() =>
            {
                Code.WriteLine($"var frame = new {modelShard.Name}ChangesFrame();");
                Code.WriteLine($"changes.AddOrGet(frame);");
                Code.WriteLine();

                Code.WriteLine($"return new Mutable{modelShard.Name}LazyModelShard()");
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

            Code.WriteLine($"public IMutable{modelShard.Name}LazyModelShard AsLoadModel(IMutableModelChanges changes)");
            Code.Block(() =>
            {
                Code.WriteLine($"var frame = new {modelShard.Name}ChangesFrame();");
                Code.WriteLine($"changes.AddOrGet(frame);");
                Code.WriteLine();

                Code.WriteLine($"return new Mutable{modelShard.Name}LazyModelShard()");
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

            Code.WriteLine($"public IMutable{modelShard.Name}LazyModelShard AsApplyModel()");
            Code.Block(() =>
            {
                Code.WriteLine($"return new Mutable{modelShard.Name}LazyModelShard()");
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

    protected override void DefineApplyMethod(ModelShard modelShard)
    {
        Code.WriteLine($"public async global::System.Threading.Tasks.Task ApplyAsync(IModel model, global::System.Threading.CancellationToken token)");
        Code.Block(() =>
        {
            Code.WriteLine($"var modelShard = model.Shard<IMutable{modelShard.Name}LazyModelShard>();");
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

    protected override void DefineModelShardSpecificClasses(ModelShard modelShard)
    {
        DefineMappingSchemaClass(modelShard);
        Code.WriteLine();
        DefineModelShardInterceptor(modelShard);
    }

    private void DefineMappingSchemaClass(ModelShard modelShard)
    {
        var visibility = GetInternalTypeVisibility(modelShard);

        Code.GeneratedClassAttributes(modelShard.Scheme.Debug);
        Code.Class(visibility, "sealed partial", $"{modelShard.Name}MappingSchema",
            [
                "MappingSchema"
            ],
            () =>
            {
                Code.WriteLine($"public {modelShard.Name}MappingSchema() : base(nameof({modelShard.Name}MappingSchema))");
                Code.Block(() =>
                {
                    Code.WriteLine("var builder = new FluentMappingBuilder(this);");
                    Code.WriteLine("ConfigureMappings(builder);");
                    Code.WriteLine("builder.Build();");
                });
                Code.WriteLine();

                Code.WriteLine("private void ConfigureMappings(FluentMappingBuilder builder)");
                Code.Block(() =>
                {
                    foreach (var entity in modelShard.Collections.Select(x => x.Entity))
                    {
                        Code.WriteLine($"SetConvertExpression<byte[], {entity.Name}>(x => new {entity.Name}(new global::System.Guid(x)));");
                        Code.WriteLine($"builder.Entity<{entity.PropertiesType}>()");
                        Code.WithIndent(c =>
                        {
                            c.WriteLine($".HasTableName({modelShard.Name}ModelShardInfo.{entity.Collection.Name}Info.Name)");
                            c.WriteLine($".HasSchemaName({modelShard.Name}ModelShardInfo.{entity.Collection.Name}Info.ShardName)");
                            c.WriteLine($".Property(p => p.EntityId)");
                            c.WithIndent(c2 =>
                            {
                                c2.WriteLine(".IsPrimaryKey()");
                                c2.WriteLine(".IsNotNull()");
                                c2.WriteLine(".HasDataType(DataType.Guid)");
                                c2.WriteLine($".HasConversion(x => x.Id, x => new {entity.Name}(x));");
                            });
                        });
                        Code.WriteLine();
                    }

                    foreach (var relation in modelShard.Relations)
                    {
                        var parentTypeName = relation.Parent.Entity.Name;
                        var childTypeName = relation.Child.Entity.Name;
                        var parentRelationType = relation.RelationType is RelationType.OneToOne or RelationType.OneToMany
                            ? $"One"
                            : $"Many";
                        var childRelationType = relation.RelationType is RelationType.OneToOne
                            ? $"One"
                            : $"Many";

                        Code.WriteLine($"builder.Entity<ParentToChild<{parentRelationType}, {parentTypeName}, {childRelationType}, {childTypeName}>>()");
                        Code.WithIndent(c =>
                        {
                            c.WriteLine($".HasTableName({modelShard.Name}ModelShardInfo.{relation.Name}Info.Name)");
                            c.WriteLine($".HasSchemaName({modelShard.Name}ModelShardInfo.{relation.Name}Info.ShardName)");
                            c.WriteLine($".Property(p => p.Parent)");
                            c.WithIndent(c2 =>
                            {
                                c2.WriteLine(".IsNotNull()");
                                c2.WriteLine(".IsPrimaryKey()");
                                c2.WriteLine($".HasColumnName(\"{parentTypeName}Id\")");
                                c2.WriteLine(".HasDataType(DataType.Guid)");
                                c2.WriteLine($".HasConversion(x => x.Id, x => new {parentTypeName}(x))");
                            });
                            c.WriteLine($".Property(p => p.Child)");
                            c.WithIndent(c2 =>
                            {
                                c2.WriteLine(".IsNotNull()");
                                if (relation.RelationType is not RelationType.OneToOne)
                                {
                                    c2.WriteLine(".IsPrimaryKey()");
                                }
                                c2.WriteLine($".HasColumnName(\"{childTypeName}Id\")");
                                c2.WriteLine(".HasDataType(DataType.Guid)");
                                c2.WriteLine($".HasConversion(x => x.Id, x => new {childTypeName}(x));");
                            });
                        });
                        Code.WriteLine();

                    }

                    Code.WriteLine("ConfigureEntityMappings(builder);");
                });
                Code.WriteLine();

                Code.WriteLine("partial void ConfigureEntityMappings(FluentMappingBuilder builder);");
            });
    }

    private void DefineModelShardInterceptor(ModelShard modelShard)
    {
        var visibility = GetInternalTypeVisibility(modelShard);

        Code.GeneratedClassAttributes(modelShard.Scheme.Debug);
        Code.Class(visibility, "sealed", $"{modelShard.Name}LazyModelShardInterceptor",
            [
                "EntityServiceInterceptor"
            ],
            () =>
            {

                foreach (var collection in modelShard.Collections)
                {
                    Code.WriteLine(DefineProperty($"IEntityCache<{collection.EntityPropertyTypes}>", collection.Name, "get;"));
                }
                Code.EmptyLine();

                Code.WriteLine($"public {modelShard.Name}LazyModelShardInterceptor(IModel model)");
                Code.Block(() =>
                {
                    Code.WriteLine($"var shard = model.Shard<I{modelShard.Name}LazyModelShard>();");
                    Code.EmptyLine();

                    foreach (var collection in modelShard.Collections)
                    {
                        Code.WriteLine($"{collection.Name} = (IEntityCache<{collection.EntityPropertyTypes}>)shard.{collection.Name};");
                    }
                });
                Code.WriteLine();

                Code.WriteLine("public override object EntityCreated(EntityCreatedEventData eventData, object entity)");
                Code.Block(() =>
                {
                    foreach (var collection in modelShard.Collections)
                    {
                        Code.WriteLine($"if (eventData.SchemaName == {modelShard.Name}ModelShardInfo.{collection.Name}Info.ShardName && eventData.TableName == {modelShard.Name}ModelShardInfo.{collection.Name}Info.Name && entity is {collection.Entity.PropertiesType} {ToCamelCase(collection.Entity.PropertiesType)})");
                        Code.Block(() =>
                        {
                            Code.WriteLine($"{collection.Name}.Cache({ToCamelCase(collection.Entity.PropertiesType)});");
                        });
                    }
                    Code.WriteLine();

                    Code.WriteLine("return base.EntityCreated(eventData, entity);");
                });
            });
    }

    protected override void DefineModelShardViewClass(ModelShard modelShard)
    {
        var visibility = GetInternalTypeVisibility(modelShard);

        Code.GeneratedClassAttributes(modelShard.Scheme.Debug);
        Code.Class(visibility, "sealed partial", $"{modelShard.Name}LazyModelShardView",
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
            Code.WriteLine($"public {modelShard.Name}LazyModelShardView(IDomainModel model)");
            Code.Block(() =>
            {
                Code.WriteLine($"var builder = new LazyModelViewBuilder<I{modelShard.Name}LazyModelShard, I{modelShard.Name}ChangesFrame>(model);");
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

            Code.WriteLine($"~{modelShard.Name}LazyModelShardView()");
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
                Code.WriteLine($"public ILazyCollectionView<{collection.EntityPropertyTypes}> {collection.Name} {{ get; private set; }}");
            }
            Code.EmptyLine();

            foreach (var relation in modelShard.Relations)
            {
                Code.WriteLine($"public ILazyRelationView<{relation.Parent.Entity.Name}, {relation.Child.Entity.Name}> {relation.Name} {{ get; private set; }}");
            }
            Code.EmptyLine();

            Code.WriteLine("public void Save(IRepository repository)");
            Code.Block(() =>
            {
                Code.WriteLine("throw new global::System.InvalidOperationException(\"Cannot save model shard's view. Call Save on the real model shard.\");");
            });
        }
    }
}
