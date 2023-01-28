using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Core;

/// <summary>
///     A model shard which can be mutated
/// </summary>
/// <typeparam name="TShard">A type of the <see cref="IModelShard"/></typeparam>
public interface ICanBeMutable<out TShard> : IModelShard
    where TShard : IModelShard
{
    /// <summary>
    ///     Returns a model shard which can be mutated
    /// </summary>
    /// <param name="features"></param>
    /// <param name="modelChanges">A model changes</param>
    /// <returns>A mutable model shard</returns>
    TShard AsMutable(Features features, IWritableModelChanges modelChanges);
}
