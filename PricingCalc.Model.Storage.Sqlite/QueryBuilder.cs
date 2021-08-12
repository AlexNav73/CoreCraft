using System.Linq;
using System.Text;
using PricingCalc.Model.Engine.Persistence;

namespace PricingCalc.Model.Storage.Sqlite
{
    internal static class QueryBuilder
    {
        internal static string IfTableExists(string name)
        {
            return $@"SELECT IIF (EXISTS (SELECT 0 FROM sqlite_master WHERE type = 'table' AND name = '{name}'), TRUE, FALSE) result";
        }

        internal static string CreateVersionTable(string versionTableName)
        {
            return @$"
CREATE TABLE IF NOT EXISTS [{versionTableName}] (
    [Name] TEXT NOT NULL UNIQUE,
    [Version] TEXT NOT NULL,

    PRIMARY KEY ([Name])
);
";
        }

        internal static string UpdateShardVersion(string versionTableName)
        {
            return $"REPLACE INTO [{versionTableName}] ([Name], [Version]) VALUES ($Name, $Version);";
        }

        internal static string Version(string versionTableName, string name)
        {
            return $"SELECT [Version] FROM [{versionTableName}] WHERE [Name] = '{name}';";
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
                    .AppendJoin(", ", scheme.Properties.Select(x => $"[{x.Name}]"))
                    .Append(") VALUES ($Id, ")
                    .AppendJoin(", ", scheme.Properties.Select(x => $"${x.Name}"))
                    .Append(");");

                return builder.ToString();
            }

            internal static string Update(Scheme scheme, string name)
            {
                var builder = new StringBuilder();

                builder.AppendFormat("UPDATE [{0}] SET ", name)
                    .AppendJoin(", ", scheme.Properties.Select(x => $"[{x.Name}] = ${x.Name}"))
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
                    .AppendJoin(", ", scheme.Properties.Select(x => $"[{x.Name}]"))
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
}
