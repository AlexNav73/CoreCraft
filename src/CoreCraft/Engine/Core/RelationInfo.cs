using System.Diagnostics.CodeAnalysis;

namespace CoreCraft.Engine.Core;

/// <summary>
///     A scheme of a relation's type. This is used instead of reflection
///     to discover all the metadata of a relation.   
/// </summary>
/// <param name="ShardName">The name of a shard</param>
/// <param name="Name">The name of a relation</param>
[ExcludeFromCodeCoverage]
public sealed record RelationInfo(string ShardName, string Name);
