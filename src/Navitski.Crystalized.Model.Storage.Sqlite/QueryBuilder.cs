using System.Text;
using Navitski.Crystalized.Model.Engine.Persistence;

namespace Navitski.Crystalized.Model.Storage.Sqlite;

internal static class QueryBuilder
{
    internal static string IfTableExists(string name)
    {
        return $@"SELECT IIF (EXISTS (SELECT 0 FROM sqlite_master WHERE type = 'table' AND name = '{name}'), TRUE, FALSE) result";
    }

    internal static string DropTable(string name)
    {
        return $"DROP TABLE IF EXISTS {name};";
    }

    internal static class Migrations
    {
        internal static string SetDatabaseVersion(long version) => $"PRAGMA user_version = {version};";

        internal const string GetDatabaseVersion = "PRAGMA user_version;";
    }

    internal static class Collections
    {
        internal static string CreateTable(Scheme scheme, string name)
        {
            var builder = new StringBuilder();

            builder.Append($"CREATE TABLE IF NOT EXISTS [{name}] (");
            builder.Append("[Id] TEXT NOT NULL UNIQUE, ");
            foreach (var property in scheme.Properties)
            {
                builder.AppendFormat("[{0}] {1}{2}, ", property.Name, SqlTypeMapper.DbTypeName(property.Type), property.IsNullable ? string.Empty : " NOT NULL");
            }
            builder.Append("PRIMARY KEY([Id])");
            builder.Append(");");

            return builder.ToString();
        }

        internal static string Insert(Scheme scheme, string name)
        {
            var builder = new StringBuilder();

            builder.AppendFormat("INSERT INTO [{0}] ([Id], ", name)
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

        internal static string Update(Scheme scheme, string name)
        {
            var builder = new StringBuilder();

            builder.AppendFormat("UPDATE [{0}] SET ", name)
#if NET5_0_OR_GREATER
                .AppendJoin(", ", scheme.Properties.Select(x => $"[{x.Name}] = ${x.Name}"))
#else
                .Append(string.Join(", ", scheme.Properties.Select(x => $"[{x.Name}] = ${x.Name}")))
#endif
                .Append(" WHERE [Id] = $Id;");

            return builder.ToString();
        }

        internal static string Delete(string name)
        {
            return $"DELETE FROM [{name}] WHERE [Id] = $Id;";
        }

        internal static string Select(Scheme scheme, string name)
        {
            var builder = new StringBuilder();

            builder.Append("SELECT [Id], ")
#if NET5_0_OR_GREATER
                .AppendJoin(", ", scheme.Properties.Select(x => $"[{x.Name}]"))
#else
                .Append(string.Join(", ", scheme.Properties.Select(x => $"[{x.Name}]")))
#endif
                .AppendFormat(" FROM [{0}];", name);

            return builder.ToString();
        }
    }

    internal static class Relations
    {
        internal static string CreateTable(string name)
        {
            return $@"
CREATE TABLE IF NOT EXISTS [{name}] (
    [ParentId] TEXT NOT NULL,
    [ChildId] TEXT NOT NULL,
    PRIMARY KEY ([ParentId], [ChildId])
);";
        }

        internal static string Insert(string name)
        {
            return $"INSERT INTO [{name}] ([ParentId], [ChildId]) VALUES ($ParentId, $ChildId);";
        }

        internal static string Delete(string name)
        {
            return $"DELETE FROM [{name}] WHERE [ParentId] = $ParentId AND [ChildId] = $ChildId;";
        }

        internal static string Select(string name)
        {
            return $"SELECT [ParentId], [ChildId] FROM [{name}];";
        }
    }
}
