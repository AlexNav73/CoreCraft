using System.Diagnostics.CodeAnalysis;

namespace Navitski.Crystalized.Model.Engine.Core;

/// <summary>
///     A description of a specific property of an entity
/// </summary>
/// <param name="Name">Name of a property</param>
/// <param name="Type">Type of a property</param>
/// <param name="IsNullable">Nullability of a value</param>
[ExcludeFromCodeCoverage]
public sealed record Property(string Name, Type Type, bool IsNullable);

/// <summary>
///     A scheme of the entity properties type. This is used instead of reflection
///     to discover all the metadata of a properties type.
/// </summary>
/// <param name="Properties">A description of each property</param>
// TODO(#6): Could we use PropertiesBag to generate queries?
[ExcludeFromCodeCoverage]
public sealed record Scheme(IReadOnlyList<Property> Properties);
