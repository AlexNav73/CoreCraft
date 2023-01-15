using System.Diagnostics.CodeAnalysis;

namespace Navitski.Crystalized.Model.Storage.Json.Model;

[ExcludeFromCodeCoverage]
internal sealed record Pair(Guid Parent, Guid Child);
