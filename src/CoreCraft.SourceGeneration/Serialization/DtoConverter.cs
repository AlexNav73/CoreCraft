namespace CoreCraft.SourceGeneration.Serialization;

internal static class DtoConverter
{
    public static ModelScheme Convert(ModelSchemeDto modelScheme)
    {
        var scheme = new ModelScheme(modelScheme.Debug);

        scheme.Shards = modelScheme.Shards.Select(x => Convert(x, scheme)).ToArray();

        return scheme;
    }

    private static ModelShard Convert(ModelShardDto modelShard, ModelScheme scheme)
    {
        var collections = new List<Collection>();
        var relations = new List<Relation>();

        var result = new ModelShard()
        {
            Name = modelShard.Name,
            Visibility = Convert(modelShard.Visibility),
            Collections = collections,
            Relations = relations,
            LoadManually = modelShard.LoadManually,
            Scheme = scheme
        };

        var entities = modelShard.Entities.Select(Convert).ToArray();

        foreach (var collectionDto in modelShard.Collections)
        {
            var entity = entities.Single(x => x.Name == collectionDto.EntityType);

            entity.Collection = new Collection(collectionDto.Name, entity, result, collectionDto.LoadManually);

            collections.Add(entity.Collection);
        }

        foreach (var relation in modelShard.Relations)
        {
            var parentEntity = entities.Single(x => x.Name == relation.Parent);
            var parentCollection = collections.Single(x => x.Entity == parentEntity);

            var childEntity = entities.Single(x => x.Name == relation.Child);
            var childCollection = collections.Single(x => x.Entity == childEntity);

            relations.Add(new Relation(relation.Name, parentCollection, childCollection, Convert(relation.RelationType), result));
        }

        return result;
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
            RelationTypeDto.ManyToMany => RelationType.ManyToMany,
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
