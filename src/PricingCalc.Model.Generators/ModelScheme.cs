namespace PricingCalc.Model.Generators;

internal record ModelScheme(IEnumerable<ModelShard> Shards);

internal record ModelShard
{
    public string Name { get; init; }

    public bool IsInternal { get; init; }

    public IEnumerable<Entity> Entities { get; init; }

    public IEnumerable<Collection> Collections { get; init; }

    public IEnumerable<Relation> Relations { get; init; }
}

internal record Collection(string Name, string Type);

internal record Relation
{
    public string Name { get; init; }

    public string ParentType { get; init; }

    public string ChildType { get; init; }

    public string ParentRelationType { get; init; }

    public string ChildRelationType { get; init; }
}

internal record Entity(string Name, IEnumerable<Property> Properties);

internal record Property(string Name, string Type, bool IsNullable, string DefaultValue);
