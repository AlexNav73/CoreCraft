﻿using System.Diagnostics.CodeAnalysis;

namespace CoreCraft.Core;

/// <summary>
///     A description of a specific property of an entity
/// </summary>
/// <param name="Name">Name of a property</param>
/// <param name="Type">Type of a property</param>
/// <param name="IsNullable">Nullability of a value</param>
[ExcludeFromCodeCoverage]
public sealed record PropertyInfo(string Name, Type Type, bool IsNullable);

/// <summary>
///     A scheme of a collection's type. This is used instead of reflection
///     to discover all the metadata of a collection.
/// </summary>
/// <param name="ShardName">The name of a shard</param>
/// <param name="Name">The name of a collection</param>
/// <param name="Properties">A description of each property</param>
// TODO(#6): Could we use PropertiesBag to generate queries?
[ExcludeFromCodeCoverage]
public sealed record CollectionInfo(string ShardName, string Name, IReadOnlyList<PropertyInfo> Properties)
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="shardName">A shard name</param>
    /// <param name="name">A collection name</param>
    public CollectionInfo(string shardName, string name)
        : this(shardName, name, Array.Empty<PropertyInfo>())
    {
    }
}
