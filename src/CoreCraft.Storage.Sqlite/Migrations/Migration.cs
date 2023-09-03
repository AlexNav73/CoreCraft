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
}
