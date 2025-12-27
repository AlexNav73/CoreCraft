using ConsoleDemoApp.Model.Entities;
using LinqToDB.Mapping;

namespace ConsoleDemoApp.Model;

/// <summary>
/// 
/// </summary>
partial class ExampleMappingSchema
{
    partial void ConfigureEntityMappings(FluentMappingBuilder builder)
    {
        builder.Entity<SecondEntityProperties>()
            .HasTableName(ExampleModelShardInfo.SecondCollectionInfo.Name)
            .HasSchemaName(ExampleModelShardInfo.FirstCollectionInfo.ShardName)
            .Property(p => p.EnumProperty)
                .IsNotColumn()
                .HasSkipOnInsert()
                .SkipOnEntityFetch()
                .HasSkipOnUpdate();
    }
}
