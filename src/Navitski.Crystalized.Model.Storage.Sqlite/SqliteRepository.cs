﻿using System.Data;
using Microsoft.Data.Sqlite;
using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Persistence;

namespace Navitski.Crystalized.Model.Storage.Sqlite;

internal sealed class SqliteRepository : DisposableBase, ISqliteRepository
{
    private SqliteConnection _connection;

    public SqliteRepository(string path)
    {
        _connection = new SqliteConnection($"Data Source={path}");
        _connection.Open();
    }

    public IDbTransaction BeginTransaction()
    {
        return _connection.BeginTransaction();
    }

    public long GetDatabaseVersion()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = QueryBuilder.Migrations.GetDatabaseVersion;

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return reader.GetInt64(0);
        }

        return 0;
    }

    public void SetDatabaseVersion(long version)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = QueryBuilder.Migrations.SetDatabaseVersion(version);
        command.ExecuteNonQuery();
    }

    public void ExecuteNonQuery(string query)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = query;
        command.ExecuteNonQuery();
    }

    public void Insert<TEntity, TProperties>(
        string name,
        IReadOnlyCollection<KeyValuePair<TEntity, TProperties>> items,
        Scheme scheme)
        where TEntity : Entity
        where TProperties : Properties
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

    public void Update<TEntity, TProperties>(string name, IReadOnlyCollection<ICollectionChange<TEntity, TProperties>> changes, Scheme scheme)
        where TEntity : Entity
        where TProperties : Properties
    {
        var commandsCache = new Dictionary<int, SqliteCommand>();

        foreach (var change in changes)
        {
            var modifiedProperties = CreatePropertiesBagForChanges(change);
            var commandKey = CreateCommandKey(modifiedProperties.Select(x => x.Key));

            SqliteCommand command;
            if (commandsCache.TryGetValue(commandKey, out var cachedCommand))
            {
                command = cachedCommand;
            }
            else
            {
                command = _connection.CreateCommand();

                var properties = scheme.Properties.Where(p => modifiedProperties.ContainsProp(p.Name)).ToArray();
                command.CommandText = QueryBuilder.Collections.Update(properties, name);

                CreateParameters(command, properties);

                commandsCache.Add(commandKey, command);
            }

            AssignValuesToParameters(command, change.Entity.Id, modifiedProperties);

            command.ExecuteNonQuery();
        }

        foreach (var command in commandsCache.Values)
        {
            command.Dispose();
        }
    }

    public void Delete<TEntity>(string name, IReadOnlyCollection<TEntity> entities)
        where TEntity : Entity
    {
        using var command = _connection.CreateCommand();
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

    public void Select<TEntity, TProperties>(string name, IMutableCollection<TEntity, TProperties> collection, Scheme scheme)
        where TEntity : Entity
        where TProperties : Properties
    {
        if (!Exists(name))
        {
            return;
        }

        using var command = _connection.CreateCommand();
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

            collection.Add(id, p => (TProperties)p.ReadFrom(bag));
        }
    }

    public void Select<TParent, TChild>(string name, IMutableRelation<TParent, TChild> relation, IEnumerable<TParent> parentCollection, IEnumerable<TChild> childCollection)
        where TParent : Entity
        where TChild : Entity
    {
        if (!Exists(name))
        {
            return;
        }

        using var command = _connection.CreateCommand();
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

    internal bool Exists(string name)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = QueryBuilder.IfTableExists(name);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return reader.GetBoolean(0);
        }

        return false;
    }

    private void ExecuteCollectionCommand<TEntity, TProperties>(string query, IReadOnlyCollection<KeyValuePair<TEntity, TProperties>> items, Scheme scheme)
        where TEntity : Entity
        where TProperties : Properties
    {
        using var command = _connection.CreateCommand();
        command.CommandText = query;
        CreateParameters(command, scheme.Properties);

        foreach (var pair in items)
        {
            var bag = new PropertiesBag();
            pair.Value.WriteTo(bag);

            AssignValuesToParameters(command, pair.Key.Id, bag);

            command.ExecuteNonQuery();
        }
    }

    private void ExecuteRelationCommand<TParent, TChild>(string query, IReadOnlyCollection<KeyValuePair<TParent, TChild>> relations)
        where TParent : Entity
        where TChild : Entity
    {
        using var command = _connection.CreateCommand();
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

    private static void CreateParameters(SqliteCommand command, IReadOnlyList<Property> properties)
    {
        var idParameter = command.CreateParameter();
        idParameter.ParameterName = "$Id";
        command.Parameters.Add(idParameter);

        for (var i = 0; i < properties.Count; i++)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = $"${properties[i].Name}";
            parameter.IsNullable = properties[i].IsNullable;
            command.Parameters.Add(parameter);
        }
    }

    private int CreateCommandKey(IEnumerable<string> strings)
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = 17;
            foreach (var str in strings)
            {
                hash = hash * 23 + str.GetHashCode();
            }
            return hash;
        }
    }

    private static PropertiesBag CreatePropertiesBagForChanges<TEntity, TProperties>(ICollectionChange<TEntity, TProperties> change)
        where TEntity : Entity
        where TProperties : Properties
    {
        var oldProperties = new PropertiesBag();
        change.OldData!.WriteTo(oldProperties);
        var newProperties = new PropertiesBag();
        change.NewData!.WriteTo(newProperties);
        var diff = oldProperties.Compare(newProperties);

        return diff;
    }

    private static void AssignValuesToParameters(SqliteCommand command, Guid id, PropertiesBag bag)
    {
        SetParameterValue(command, "Id", id);

        foreach (var property in bag)
        {
            SetParameterValue(command, property.Key, property.Value);
        }
    }

    private static void SetParameterValue(SqliteCommand command, string parameter, object? value)
    {
        if (value != null)
        {
            command.Parameters[$"${parameter}"].Value = value;
        }
        else
        {
            command.Parameters[$"${parameter}"].Value = DBNull.Value;
        }
    }

    protected override void DisposeManagedObjects()
    {
        _connection.Close();
        _connection.Dispose();
        _connection = null!;

        // Connections are pooled to speedup creation of the new connections.
        // In our case we don't need keep closed connections because a database file
        // could be moved/renamed/deleted afterwards. Opened connection holds a handle
        // to the file and blocks it so no other process or thread can access it.
        // Here we drop all connections just to make sure that if we need to do something
        // with a database file - we won't be blocked
        SqliteConnection.ClearAllPools();
    }
}
