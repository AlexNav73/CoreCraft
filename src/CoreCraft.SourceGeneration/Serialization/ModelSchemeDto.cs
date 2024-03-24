namespace CoreCraft.SourceGeneration.Serialization;

internal sealed record ModelSchemeDto(IEnumerable<ModelShardDto> Shards);

internal sealed record ModelShardDto
{
    public string Name { get; init; }

    public bool LoadManually { get; init; }

    public VisibilityDto Visibility { get; init; }

    public IEnumerable<EntityDto> Entities { get; init; }

    public IEnumerable<CollectionDto> Collections { get; init; }

    public IEnumerable<RelationDto> Relations { get; init; }
}

internal sealed record CollectionDto(string Name, string EntityType, bool LoadManually = false);

internal sealed record RelationDto
{
    public string Name { get; init; }

    public string ParentType { get; init; }

    public string ChildType { get; init; }

    public RelationTypeDto ParentRelationType { get; init; }

    public RelationTypeDto ChildRelationType { get; init; }
}

internal enum VisibilityDto
{
    Interfaces,
    Implementations,
    All,
}

internal enum RelationTypeDto
{
    OneToOne,
    OneToMany
}

internal sealed record EntityDto(string Name, IEnumerable<PropertyDto> Properties);

internal sealed record PropertyDto(string Name, string Type, bool IsNullable, string DefaultValue);
