namespace CoreCraft.Generators;

internal sealed record ModelScheme(IEnumerable<ModelShard> Shards);

internal sealed record ModelShard
{
    public string Name { get; init; }

    public Visibility Visibility { get; init; }

    public IEnumerable<Entity> Entities { get; init; }

    public IEnumerable<Collection> Collections { get; init; }

    public IEnumerable<Relation> Relations { get; init; }
}

internal sealed record Collection(string Name, string EntityType);

internal sealed record Relation
{
    public string Name { get; init; }

    public string ParentType { get; init; }

    public string ChildType { get; init; }

    public RelationType ParentRelationType { get; init; }

    public RelationType ChildRelationType { get; init; }
}

internal enum Visibility
{
    Interfaces,
    Implementations,
    All,
}

internal enum RelationType
{
    OneToOne,
    OneToMany
}

internal sealed record Entity(string Name, IEnumerable<Property> Properties);

internal sealed record Property(string Name, string Type, bool IsNullable, string DefaultValue);
