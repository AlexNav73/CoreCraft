namespace Navitski.Crystalized.Model.Engine.Core;

/// <summary>
///     Features are supported by model shards and can be used
///     wile calling <see cref="ICanBeMutable{TShard}.AsMutable(Features, ChangesTracking.IWritableModelChanges)"/>
/// </summary>
[Flags]
public enum Features
{
    /// <summary>
    ///     Collections and relations will be copied before changing
    /// </summary>
    Copy = 1,
    /// <summary>
    ///     All changes to the collections and relations will be tracked
    /// </summary>
    Track = 2 | Copy
}
