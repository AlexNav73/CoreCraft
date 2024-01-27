using System.Data;
using System.Text;
using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Exceptions;
using Microsoft.Data.Sqlite;
using static CoreCraft.Storage.Sqlite.QueryBuilder;

namespace CoreCraft.Storage.Sqlite;

internal sealed class SqliteRepository : DisposableBase, ISqliteRepository
{
    private const string ParentIdParamName = "$ParentId";
    private const string ChildIdParamName = "$ChildId";
    private const string IdParamName = "$Id";

    private readonly Action<string>? _loggingAction;

    private SqliteConnection _connection;

    public SqliteRepository(string path, Action<string>? loggingAction = null)
    {
        _connection = new SqliteConnection($"Data Source={path}");
        _connection.Open();
        _loggingAction = loggingAction;
    }

    public IDbTransaction BeginTransaction()
    {
        return _connection.BeginTransaction();
    }

    public int GetDatabaseVersion()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = QueryBuilder.Migrations.GetDatabaseVersion;

        Log(command);

        var version = (long?)command.ExecuteScalar();

        return ((int?)version) ?? 0;
    }

    public void SetDatabaseVersion(int version)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = QueryBuilder.Migrations.SetDatabaseVersion(version);
        Log(command);
        command.ExecuteNonQuery();
    }

    public void ExecuteNonQuery(string query)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = query;
        Log(command);
        command.ExecuteNonQuery();
    }

    public void Save<TEntity, TProperties>(ICollectionChangeSet<TEntity, TProperties> changes)
        where TEntity : Entity
        where TProperties : Properties
    {
        if (!changes.HasChanges())
        {
            return;
        }

        if (!Exists(changes.Info))
        {
            ExecuteNonQuery(Collections.CreateTable(changes.Info));
        }

        var updateCommandCache = new Dictionary<int, SqliteCommand>();
        using var insertCommand = CreateCommand(Collections.Insert(changes.Info), changes.Info.Properties);
        using var deleteCommand = CreateCollectionDeleteCommand(changes.Info);

        foreach (var change in changes)
        {
            switch (change.Action)
            {
                case CollectionAction.Add:
                    Insert(insertCommand, change.Entity, change.NewData!);
                    break;

                case CollectionAction.Modify:
                    Update(changes.Info, change, updateCommandCache);
                    break;

                case CollectionAction.Remove:
                    Delete(deleteCommand, change.Entity);
                    break;
            }
        }

        foreach (var command in updateCommandCache.Values)
        {
            command.Dispose();
        }
    }

    public void Save<TParent, TChild>(IRelationChangeSet<TParent, TChild> changes)
        where TParent : Entity
        where TChild : Entity
    {
        if (!changes.HasChanges())
        {
            return;
        }

        if (!Exists(changes.Info))
        {
            ExecuteNonQuery(Relations.CreateTable(changes.Info));
        }

        using var insertCommand = CreateRelationCommand(Relations.Insert(changes.Info));
        using var deleteCommand = CreateRelationCommand(Relations.Delete(changes.Info));

        foreach (var change in changes)
        {
            switch (change.Action)
            {
                case RelationAction.Linked:
                    ExecuteRelationCommand(insertCommand, change.Parent, change.Child);
                    break;
                case RelationAction.Unlinked:
                    ExecuteRelationCommand(deleteCommand, change.Parent, change.Child);
                    break;
            }
        }
    }

    public void Save<TEntity, TProperties>(ICollection<TEntity, TProperties> collection)
        where TEntity : Entity
        where TProperties : Properties
    {
        if (collection.Count == 0)
        {
            return;
        }

        if (!Exists(collection.Info))
        {
            ExecuteNonQuery(Collections.CreateTable(collection.Info));
        }

        using var insertCommand = CreateCommand(Collections.Insert(collection.Info), collection.Info.Properties);

        foreach (var (entity, properties) in collection.Pairs())
        {
            Insert(insertCommand, entity, properties);
        }
    }

    public void Save<TParent, TChild>(IRelation<TParent, TChild> relation)
        where TParent : Entity
        where TChild : Entity
    {
        if (!relation.Any())
        {
            return;
        }

        if (!Exists(relation.Info))
        {
            ExecuteNonQuery(Relations.CreateTable(relation.Info));
        }

        using var insertCommand = CreateRelationCommand(Relations.Insert(relation.Info));

        foreach (var parent in relation)
        {
            foreach (var child in relation.Children(parent))
            {
                ExecuteRelationCommand(insertCommand, parent, child);
            }
        }
    }

    public void Load<TEntity, TProperties>(IMutableCollection<TEntity, TProperties> collection)
        where TEntity : Entity
        where TProperties : Properties
    {
        if (collection.Count != 0)
        {
            throw new NonEmptyModelException($"The [{collection.Info.ShardName}.{collection.Info.Name}] is not empty. Clear or recreate the model before loading data");
        }

        if (!Exists(collection.Info))
        {
            return;
        }

        using var command = CreateCommand(Collections.Select(collection.Info));

        Log(command);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var bag = new PropertiesBag();
            var id = reader.GetGuid(0);

            for (var i = 0; i < collection.Info.Properties.Count; i++)
            {
                bag.Write(collection.Info.Properties[i].Name, Convert.ChangeType(reader.GetValue(i + 1), collection.Info.Properties[i].Type));
            }

            collection.Add(id, p => (TProperties)p.ReadFrom(bag));
        }
    }

    public void Load<TParent, TChild>(IMutableRelation<TParent, TChild> relation, IEnumerable<TParent> parents, IEnumerable<TChild> children)
        where TParent : Entity
        where TChild : Entity
    {
        if (relation.Any())
        {
            throw new NonEmptyModelException($"The [{relation.Info.ShardName}.{relation.Info.Name}] is not empty. Clear or recreate the model before loading data");
        }

        if (!Exists(relation.Info))
        {
            return;
        }

        using var command = _connection.CreateCommand();
        command.CommandText = Relations.Select(relation.Info);

        Log(command);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var parentId = reader.GetGuid(0);
            var childId = reader.GetGuid(1);

            var parent = parents.Single(x => x.Id == parentId);
            var child = children.Single(x => x.Id == childId);

            relation.Add(parent, child);
        }
    }

    internal bool Exists(CollectionInfo collection)
    {
        return Exists(InferName(collection));
    }

    internal bool Exists(RelationInfo relation)
    {
        return Exists(InferName(relation));
    }

    internal IEnumerable<SqliteColumnInfo> QueryTableColumns(CollectionInfo collection)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info([{InferName(collection)}]);";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var columnName = reader.GetString(1);
            var columnType = reader.GetString(2);
            var columnIsNotNullable = reader.GetBoolean(3);
            var columnDefaultValue = reader.GetValue(4);

            yield return new SqliteColumnInfo(columnName, columnType, columnIsNotNullable, columnDefaultValue);
        }
    }

    private void Insert<TEntity, TProperties>(SqliteCommand command, TEntity entity, TProperties properties)
        where TEntity : Entity
        where TProperties : Properties
    {
        var bag = new PropertiesBag();
        properties.WriteTo(bag);

        AssignValuesToParameters(command, entity.Id, bag);

        Log(command);

        command.ExecuteNonQuery();
    }

    private void Update<TEntity, TProperties>(CollectionInfo scheme, ICollectionChange<TEntity, TProperties> change, IDictionary<int, SqliteCommand> commandsCache)
        where TEntity : Entity
        where TProperties : Properties
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
            var properties = scheme.Properties.Where(p => modifiedProperties.ContainsProp(p.Name)).ToArray();

            command = CreateCommand(Collections.Update(properties, scheme), properties);

            commandsCache.Add(commandKey, command);
        }

        AssignValuesToParameters(command, change.Entity.Id, modifiedProperties);

        Log(command);

        command.ExecuteNonQuery();
    }

    private void Delete<TEntity>(SqliteCommand command, TEntity entity)
        where TEntity : Entity
    {
        command.Parameters[IdParamName].Value = entity.Id;

        Log(command);

        command.ExecuteNonQuery();
    }

    private bool Exists(string name)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = IfTableExists(name);

        Log(command);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return reader.GetBoolean(0);
        }

        return false;
    }

    private SqliteCommand CreateCommand(string query)
    {
        var command = _connection.CreateCommand();
        command.CommandText = query;

        return command;
    }

    private SqliteCommand CreateCommand(string query, IReadOnlyList<PropertyInfo> properties)
    {
        var command = CreateCommand(query);

        CreateParameters(command, properties);

        return command;
    }

    private SqliteCommand CreateRelationCommand(string query)
    {
        var command = CreateCommand(query);

        var parentParameter = command.CreateParameter();
        parentParameter.ParameterName = ParentIdParamName;
        command.Parameters.Add(parentParameter);
        var childParameter = command.CreateParameter();
        childParameter.ParameterName = ChildIdParamName;
        command.Parameters.Add(childParameter);

        return command;
    }

    private SqliteCommand CreateCollectionDeleteCommand(CollectionInfo scheme)
    {
        var command = CreateCommand(Collections.Delete(scheme));
        var parameter = command.CreateParameter();
        parameter.ParameterName = IdParamName;
        command.Parameters.Add(parameter);

        return command;
    }

    private void ExecuteRelationCommand<TParent, TChild>(SqliteCommand command, TParent parent, TChild child)
        where TParent : Entity
        where TChild : Entity
    {
        command.Parameters[ParentIdParamName].Value = parent.Id;
        command.Parameters[ChildIdParamName].Value = child.Id;

        Log(command);

        command.ExecuteNonQuery();
    }

    private void Log(SqliteCommand command)
    {
        if (_loggingAction != null)
        {
            var builder = new StringBuilder(command.CommandText);

            for (var i = 0; i < command.Parameters.Count; i++)
            {
                builder.Replace(command.Parameters[i].ParameterName, command.Parameters[i].Value?.ToString());
            }

            _loggingAction(builder.ToString());
        }
    }

    private static void CreateParameters(SqliteCommand command, IReadOnlyList<PropertyInfo> properties)
    {
        var idParameter = command.CreateParameter();
        idParameter.ParameterName = IdParamName;
        command.Parameters.Add(idParameter);

        for (var i = 0; i < properties.Count; i++)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = $"${properties[i].Name}";
            parameter.IsNullable = properties[i].IsNullable;
            command.Parameters.Add(parameter);
        }
    }

    private static int CreateCommandKey(IEnumerable<string> strings)
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
