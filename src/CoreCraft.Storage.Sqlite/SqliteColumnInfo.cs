using System.Diagnostics.CodeAnalysis;

namespace CoreCraft.Storage.Sqlite;

[ExcludeFromCodeCoverage]
internal sealed record class SqliteColumnInfo(
    string Name,
    string Type,
    bool IsNotNull,
    object? DefaultValue);
