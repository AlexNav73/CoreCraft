namespace CoreCraft.Engine.ChangesTracking;

/// <summary>
///     Action performed on a collection
/// </summary>
public enum CollectionAction
{
    /// <summary>
    ///     An item was added to the collection
    /// </summary>
    Add,
    /// <summary>
    ///     An item was removed from the collection
    /// </summary>
    Remove,
    /// <summary>
    ///     An item from the collection was modified
    /// </summary>
    Modify
}
