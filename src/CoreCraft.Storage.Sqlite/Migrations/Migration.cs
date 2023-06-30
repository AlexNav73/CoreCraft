namespace CoreCraft.Storage.Sqlite.Migrations;

/// <inheritdoc cref="IMigration"/>
public abstract class Migration : IMigration
{
    /// <summary>
    ///     Ctor
    /// </summary>
    protected Migration(long version)
    {
        Version = version;
    }

    /// <inheritdoc />
    public long Version { get; }

    /// <inheritdoc />
    public abstract void Migrate(IMigrator migrator);
}
