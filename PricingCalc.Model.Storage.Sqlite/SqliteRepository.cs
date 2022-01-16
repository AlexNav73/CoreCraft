using System.Data.Common;
using Microsoft.Data.Sqlite;
using PricingCalc.Model.Engine.Core;
using PricingCalc.Model.Engine.Persistence;

namespace PricingCalc.Model.Storage.Sqlite;

internal class SqliteRepository : DisposableBase, IRepository
{
    private readonly SqliteConnection _connection;

    public SqliteRepository(string path)
    {
        _connection = new SqliteConnection($"Filename={path}");
        _connection.Open();
    }

    public DbTransaction BeginTransaction()
    {
        return _connection.BeginTransaction();
    }

    public (long timestamp, string name)? GetLatestigration()
    {
        if (Exists("_MigrationHistory"))
        {
            var command = _connection.CreateCommand();
            command.CommandText = QueryBuilder.Migrations.GetLatestMigration;

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return (reader.GetInt64(0), reader.GetString(1));
            }
        }

        return null;
    }

    public void UpdateLatestMigration(long timestamp, string name)
    {
        ExecuteNonQuery(QueryBuilder.Migrations.CreateMigrationTable);
        ExecuteNonQuery(QueryBuilder.Migrations.ClearMigrationTable);

        var command = _connection.CreateCommand();
        command.CommandText = QueryBuilder.Migrations.InsertMigration;
        var nameParam = command.Parameters.Add("$Timestamp", SqliteType.Integer);
        nameParam.Value = timestamp;
        var versionParam = command.Parameters.Add("$Name", SqliteType.Text);
        versionParam.Value = name;
        command.ExecuteNonQuery();
    }

    public void ExecuteNonQuery(string commandText)
    {
        var command = _connection.CreateCommand();
        command.CommandText = commandText;
        command.ExecuteNonQuery();
    }

    public void Insert<TEntity, TData>(
        string name,
        IReadOnlyCollection<KeyValuePair<TEntity, TData>> items,
        Scheme scheme)
        where TEntity : Entity
        where TData : Properties
    {
        ExecuteNonQuery(QueryBuilder.Collections.CreateTable(scheme, name));
        ExecuteCollectionCommand(QueryBuilder.Collections.Insert(scheme, name), items, scheme);
    }

    public void Insert<TParent, TChild>(
        string name,
        IReadOnlyCollection<KeyValuePair<TParent, TChild>> relations)
        where TParent : Entity
        where TChild : Entity
    {
        ExecuteNonQuery(QueryBuilder.Relations.CreateTable(name));
        ExecuteRelationCommand(QueryBuilder.Relations.Insert(name), relations);
    }

    public void Update<TEntity, TData>(string name, IReadOnlyCollection<KeyValuePair<TEntity, TData>> items, Scheme scheme)
        where TEntity : Entity
        where TData : Properties
    {
        ExecuteCollectionCommand(QueryBuilder.Collections.Update(scheme, name), items, scheme);
    }

    public void Delete<TEntity>(string name, IReadOnlyCollection<TEntity> entities)
        where TEntity : Entity
    {
        var command = _connection.CreateCommand();
        command.CommandText = QueryBuilder.Collections.Delete(name);
        var parameter = command.CreateParameter();
        parameter.ParameterName = "$Id";
        command.Parameters.Add(parameter);

        foreach (var entity in entities)
        {
            parameter.Value = entity.Id;

            command.ExecuteNonQuery();
        }
    }

    public void Delete<TParent, TChild>(string name, IReadOnlyCollection<KeyValuePair<TParent, TChild>> relations)
        where TParent : Entity
        where TChild : Entity
    {
        ExecuteRelationCommand(QueryBuilder.Relations.Delete(name), relations);
    }

    public void Select<TEntity, TData>(string name, IMutableCollection<TEntity, TData> collection, Scheme scheme)
        where TEntity : Entity
        where TData : Properties
    {
        if (!Exists(name))
        {
            return;
        }

        var command = _connection.CreateCommand();
        command.CommandText = QueryBuilder.Collections.Select(scheme, name);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var bag = new PropertiesBag();
            var id = reader.GetGuid(0);

            for (var i = 0; i < scheme.Properties.Count; i++)
            {
                bag.Write(scheme.Properties[i].Name, Convert.ChangeType(reader.GetValue(i + 1), scheme.Properties[i].Type));
            }

            collection.Add(id, p => (TData)p.ReadFrom(bag));
        }
    }

    public void Select<TParent, TChild>(string name, IMutableRelation<TParent, TChild> relation, IEntityCollection<TParent> parentCollection, IEntityCollection<TChild> childCollection)
        where TParent : Entity
        where TChild : Entity
    {
        if (!Exists(name))
        {
            return;
        }

        var command = _connection.CreateCommand();
        command.CommandText = QueryBuilder.Relations.Select(name);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var parentId = reader.GetGuid(0);
            var childId = reader.GetGuid(1);

            var parent = parentCollection.Single(x => x.Id == parentId);
            var child = childCollection.Single(x => x.Id == childId);

            relation.Add(parent, child);
        }
    }

    private void ExecuteCollectionCommand<TEntity, TData>(string query, IReadOnlyCollection<KeyValuePair<TEntity, TData>> items, Scheme scheme)
        where TEntity : Entity
        where TData : Properties
    {
        var command = _connection.CreateCommand();
        command.CommandText = query;
        var parameters = CreateParameters(command, scheme);

        foreach (var pair in items)
        {
            AssignValuesToParameters(parameters, pair.Key, pair.Value);

            command.ExecuteNonQuery();
        }
    }

    private void ExecuteRelationCommand<TParent, TChild>(string query, IReadOnlyCollection<KeyValuePair<TParent, TChild>> relations)
        where TParent : Entity
        where TChild : Entity
    {
        var command = _connection.CreateCommand();
        command.CommandText = query;

        var parentParameter = command.CreateParameter();
        parentParameter.ParameterName = "$ParentId";
        command.Parameters.Add(parentParameter);
        var childParameter = command.CreateParameter();
        childParameter.ParameterName = "$ChildId";
        command.Parameters.Add(childParameter);

        foreach (var pair in relations)
        {
            parentParameter.Value = pair.Key.Id;
            childParameter.Value = pair.Value.Id;

            command.ExecuteNonQuery();
        }
    }

    private static IDictionary<string, SqliteParameter> CreateParameters(SqliteCommand command, Scheme scheme)
    {
        var parameters = new Dictionary<string, SqliteParameter>();

        var idParameter = command.CreateParameter();
        idParameter.ParameterName = "$Id";
        parameters.Add("Id", idParameter);
        command.Parameters.Add(idParameter);

        for (var i = 0; i < scheme.Properties.Count; i++)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = $"${scheme.Properties[i].Name}";
            parameter.IsNullable = scheme.Properties[i].IsNullable;
            parameters.Add(scheme.Properties[i].Name, parameter);
            command.Parameters.Add(parameter);
        }

        return parameters;
    }

    private static void AssignValuesToParameters<TEntity, TData>(IDictionary<string, SqliteParameter> parameters, TEntity entity, TData data)
        where TEntity : Entity
        where TData : Properties
    {
        parameters["Id"].Value = entity.Id;

        var bag = new PropertiesBag();
        data.WriteTo(bag);

        foreach (var property in bag)
        {
            if (property.Value != null)
            {
                parameters[property.Key].Value = property.Value;
            }
            else
            {
                parameters[property.Key].Value = DBNull.Value;
            }
        }
    }

    private bool Exists(string name)
    {
        var command = _connection.CreateCommand();
        command.CommandText = QueryBuilder.IfTableExists(name);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return reader.GetBoolean(0);
        }

        return false;
    }

    protected override void DisposeManagedObjects()
    {
        _connection.Close();
        _connection.Dispose();
    }
}
