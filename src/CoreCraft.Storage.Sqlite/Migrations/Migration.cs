using CoreCraft.Core;

namespace CoreCraft.Storage.Sqlite.Migrations;

/// <inheritdoc cref="IMigration"/>
public abstract class Migration : IMigration
{
    /// <summary>
    ///     Ctor
    /// </summary>
    protected Migration(int version)
    {
        Version = version;
    }

    /// <inheritdoc />
    public int Version { get; }

    /// <inheritdoc />
    public abstract void Migrate(IMigrator migrator);

    /// <summary>
    ///     Gets the name of a table from the provided collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <returns>The name of the table.</returns>
    protected string NameOf(CollectionInfo collection)
    {
        return QueryBuilder.InferName(collection);
    }

    /// <summary>
    ///     Gets the name of a table from the provided relation.
    /// </summary>
    /// <param name="relation">The relation.</param>
    /// <returns>The name of the table.</returns>
    protected string NameOf(RelationInfo relation)
    {
        return QueryBuilder.InferName(relation);
    }
}
