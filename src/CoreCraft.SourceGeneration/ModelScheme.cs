namespace CoreCraft.SourceGeneration;

internal sealed record ModelScheme(bool Debug)
{
    public IEnumerable<ModelShard> Shards { get; set; }
}

internal sealed record ModelShard
{
    public string Name { get; init; }

    public bool LoadManually { get; init; }

    public Visibility Visibility { get; init; }

    public ModelScheme Scheme { get; init; }

    public IEnumerable<Collection> Collections { get; init; }

    public IEnumerable<Relation> Relations { get; init; }
}

internal sealed record Collection(string Name, Entity Entity, ModelShard Shard, bool LoadManually = false)
{
    public string Type => $"Collection<{Entity.Name}, {Entity.PropertiesType}>";

    public string ViewType => $"ICollectionView<{Entity.Name}, {Entity.PropertiesType}>";

    public string MutableType => $"Mutable{Type}";

    public string ChangesType => $"CollectionChangeSet<{Entity.Name}, {Entity.PropertiesType}>";
}

internal sealed record Relation(string Name, Collection Parent, Collection Child, RelationType RelationType, ModelShard Shard)
{
    public string Type => $"Relation<{Parent.Entity.Name}, {Child.Entity.Name}>";

    public string ViewType => $"IRelationView<{Parent.Entity.Name}, {Child.Entity.Name}>";

    public string MutableType => $"Mutable{Type}";

    public string ChangesType => $"RelationChangeSet<{Parent.Entity.Name}, {Child.Entity.Name}>";

    public string ParentRelationType => RelationType switch
    {
        RelationType.OneToOne => "OneToOne",
        RelationType.OneToMany or RelationType.ManyToMany => "OneToMany",
        _ => throw new NotSupportedException($"RelationType [{RelationType}] is not supported"),
    };

    public string ChildRelationType => RelationType switch
    {
        RelationType.OneToOne or RelationType.OneToMany => "OneToOne",
        RelationType.ManyToMany => "OneToMany",
        _ => throw new NotSupportedException($"RelationType [{RelationType}] is not supported"),
    };
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
    OneToMany,
    ManyToMany
}

internal sealed record Entity(string Name, IEnumerable<Property> Properties)
{
    public Collection Collection { get; set; }

    public string PropertiesType => $"{Name}Properties";
}

internal sealed record Property(string Name, string Type, bool IsNullable, string DefaultValue)
{
    public string Define(string accessors)
    {
        return Define(IsNullable ? $"{Type}?" : Type, Name, accessors);
    }

    private static string Define(string type, string name, string accessors = "get; private set;")
    {
        return string.Join(" ", type, name, "{", accessors, "}").Trim();
    }
}
