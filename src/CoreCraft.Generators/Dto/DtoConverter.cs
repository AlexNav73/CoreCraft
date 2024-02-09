namespace CoreCraft.Generators.Dto;

internal class DtoConverter
{
    public static ModelScheme Convert(ModelSchemeDto modelScheme)
    {
        return new ModelScheme(modelScheme.Shards.Select(Convert).ToArray());
    }

    private static ModelShard Convert(ModelShardDto modelShard)
    {
        var collections = new List<Collection>();
        var relations = new List<Relation>();

        var entities = modelShard.Entities.Select(Convert).ToArray();

        foreach (var collection in modelShard.Collections)
        {
            var entity = entities.Single(x => x.Name == collection.EntityType);

            collections.Add(new Collection(collection.Name, entity, collection.DeferLoading));
        }

        foreach (var relation in modelShard.Relations)
        {
            var parentEntity = entities.Single(x => x.Name == relation.ParentType);
            var parentCollection = collections.Single(x => x.Entity == parentEntity);

            var childEntity = entities.Single(x => x.Name == relation.ChildType);
            var childCollection = collections.Single(x => x.Entity == childEntity);

            relations.Add(new Relation()
            {
                Name = relation.Name,
                ChildRelationType = Convert(relation.ChildRelationType),
                ParentRelationType = Convert(relation.ParentRelationType),
                Parent = parentCollection,
                Child = childCollection
            });
        }

        return new ModelShard()
        {
            Name = modelShard.Name,
            Visibility = Convert(modelShard.Visibility),
            Collections = collections,
            Relations = relations
        };
    }

    private static Entity Convert(EntityDto entity)
    {
        return new Entity(entity.Name, entity.Properties.Select(Convert).ToArray());
    }

    private static Property Convert(PropertyDto property)
    {
        return new Property(property.Name, property.Type, property.IsNullable, property.DefaultValue);
    }

    private static RelationType Convert(RelationTypeDto relationType)
    {
        return relationType switch
        {
            RelationTypeDto.OneToOne => RelationType.OneToOne,
            RelationTypeDto.OneToMany => RelationType.OneToMany,
            _ => throw new NotSupportedException(),
        };
    }

    private static Visibility Convert(VisibilityDto visibility)
    {
        return visibility switch
        {
            VisibilityDto.All => Visibility.All,
            VisibilityDto.Implementations => Visibility.Implementations,
            VisibilityDto.Interfaces => Visibility.Interfaces,
            _ => throw new NotSupportedException()
        };
    }
}
