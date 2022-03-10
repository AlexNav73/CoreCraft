namespace Navitski.Crystalized.Model.Engine.Persistence;

public record Property(string Name, Type Type, bool IsNullable);

// TODO(#6): Could we use PropertiesBag to generate queries?
public record Scheme(IList<Property> Properties);
