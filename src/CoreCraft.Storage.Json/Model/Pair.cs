using System.Diagnostics.CodeAnalysis;

namespace CoreCraft.Storage.Json.Model;

[ExcludeFromCodeCoverage]
internal sealed record Pair(Guid Parent, Guid Child);
