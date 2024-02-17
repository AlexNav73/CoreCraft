namespace CoreCraft.Generators;

internal sealed record ModelScheme(IEnumerable<ModelShard> Shards);

internal sealed record ModelShard
{
    public string Name { get; init; }

    public bool LoadManually { get; init; }

    public Visibility Visibility { get; init; }

    public IEnumerable<Collection> Collections { get; init; }

    public IEnumerable<Relation> Relations { get; init; }
}

internal sealed record Collection(string Name, Entity Entity, bool LoadManually = false)
{
    public string Type => $"Collection<{Entity.Name}, {Entity.PropertiesType}>";

    public string MutableType => $"Mutable{Type}";

    public string ChangesType => $"CollectionChangeSet<{Entity.Name}, {Entity.PropertiesType}>";
}

internal sealed record Relation
{
    public string Name { get; init; }

    public Collection Parent { get; init; }

    public Collection Child { get; init; }

    public RelationType ParentRelationType { get; init; }

    public RelationType ChildRelationType { get; init; }

    public string Type => $"Relation<{Parent.Entity.Name}, {Child.Entity.Name}>";

    public string MutableType => $"Mutable{Type}";

    public string ChangesType => $"RelationChangeSet<{Parent.Entity.Name}, {Child.Entity.Name}>";
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

internal sealed record Entity(string Name, IEnumerable<Property> Properties)
{
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
