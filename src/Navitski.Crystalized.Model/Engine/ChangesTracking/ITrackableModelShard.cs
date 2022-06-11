namespace Navitski.Crystalized.Model.Engine.ChangesTracking;

/// <summary>
///     A model shard which can track modifications
/// </summary>
/// <typeparam name="TShard">A type of the <see cref="IModelShard"/></typeparam>
public interface ITrackableModelShard<out TShard> : IModelShard
    where TShard : IModelShard
{
    /// <summary>
    ///     Returns a model shard with the same type, but which can track modifications of itself
    /// </summary>
    /// <param name="modelChanges">A collection of model changes to register a <see cref="IModelChanges"/> instance of the model shard</param>
    /// <returns>A tracking model shard</returns>
    TShard AsTrackable(IWritableModelChanges modelChanges);
}
