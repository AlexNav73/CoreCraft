using System.Text;
using CoreCraft.Core;

namespace CoreCraft.Storage.Sqlite;

internal static class QueryBuilder
{
    internal static string InferName(CollectionInfo scheme)
    {
        return $"{scheme.ShardName}.{scheme.Name}";
    }

    internal static string InferName(RelationInfo scheme)
    {
        return $"{scheme.ShardName}.{scheme.Name}";
    }

    internal static string IfTableExists(string name)
    {
        return $@"SELECT IIF (EXISTS (SELECT 0 FROM sqlite_master WHERE type = 'table' AND name = '{name}'), TRUE, FALSE) result";
    }

    internal static string DropTable(string name)
    {
        return $"DROP TABLE IF EXISTS [{name}];";
    }

    internal static class Migrations
    {
        internal static string SetDatabaseVersion(long version) => $"PRAGMA user_version = {version};";

        internal const string GetDatabaseVersion = "PRAGMA user_version;";
    }

    internal static class Collections
    {
        internal static string CreateTable(CollectionInfo scheme)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"CREATE TABLE IF NOT EXISTS [{InferName(scheme)}] (");
            builder.AppendLine("   [Id] TEXT NOT NULL UNIQUE, ");
            foreach (var property in scheme.Properties)
            {
                builder.AppendFormat("   [{0}] {1}{2}, ", property.Name, SqlTypeMapper.DbTypeName(property.Type), property.IsNullable ? string.Empty : " NOT NULL");
                builder.AppendLine();
            }
            builder.AppendLine("   PRIMARY KEY([Id])");
            builder.AppendLine(");");

            return builder.ToString();
        }

        internal static string Insert(CollectionInfo scheme)
        {
            var builder = new StringBuilder();

            builder.AppendFormat("INSERT INTO [{0}] ([Id], ", InferName(scheme))
#if NET5_0_OR_GREATER
                .AppendJoin(", ", scheme.Properties.Select(x => $"[{x.Name}]"))
#else
                .Append(string.Join(", ", scheme.Properties.Select(x => $"[{x.Name}]")))
#endif
                .Append(") VALUES ($Id, ")
#if NET5_0_OR_GREATER
                .AppendJoin(", ", scheme.Properties.Select(x => $"${x.Name}"))
#else
                .Append(string.Join(", ", scheme.Properties.Select(x => $"${x.Name}")))
#endif
                .Append(");");

            return builder.ToString();
        }

        internal static string Update(IEnumerable<Property> properties, CollectionInfo scheme)
        {
            var builder = new StringBuilder();

            builder.AppendFormat("UPDATE [{0}] SET ", InferName(scheme))
#if NET5_0_OR_GREATER
                .AppendJoin(", ", properties.Select(x => $"[{x.Name}] = ${x.Name}"))
#else
                .Append(string.Join(", ", properties.Select(x => $"[{x.Name}] = ${x.Name}")))
#endif
                .Append(" WHERE [Id] = $Id;");

            return builder.ToString();
        }

        internal static string Delete(CollectionInfo scheme)
        {
            return $"DELETE FROM [{InferName(scheme)}] WHERE [Id] = $Id;";
        }

        internal static string Select(CollectionInfo scheme)
        {
            var builder = new StringBuilder();

            builder.Append("SELECT [Id], ")
#if NET5_0_OR_GREATER
                .AppendJoin(", ", scheme.Properties.Select(x => $"[{x.Name}]"))
#else
                .Append(string.Join(", ", scheme.Properties.Select(x => $"[{x.Name}]")))
#endif
                .AppendFormat(" FROM [{0}];", InferName(scheme));

            return builder.ToString();
        }
    }

    internal static class Relations
    {
        internal static string CreateTable(RelationInfo scheme)
        {
            return $@"
CREATE TABLE IF NOT EXISTS [{InferName(scheme)}] (
    [ParentId] TEXT NOT NULL,
    [ChildId] TEXT NOT NULL,
    PRIMARY KEY ([ParentId], [ChildId])
);";
        }

        internal static string Insert(RelationInfo scheme)
        {
            return $"INSERT INTO [{InferName(scheme)}] ([ParentId], [ChildId]) VALUES ($ParentId, $ChildId);";
        }

        internal static string Delete(RelationInfo scheme)
        {
            return $"DELETE FROM [{InferName(scheme)}] WHERE [ParentId] = $ParentId AND [ChildId] = $ChildId;";
        }

        internal static string Select(RelationInfo scheme)
        {
            return $"SELECT [ParentId], [ChildId] FROM [{InferName(scheme)}];";
        }
    }
}
