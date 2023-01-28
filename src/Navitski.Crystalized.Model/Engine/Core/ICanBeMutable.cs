using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Core;

/// <summary>
///     A model shard which can be mutated
/// </summary>
/// <remarks>
///     <see cref="ICanBeMutable{TShard}"/> and <see cref="ICanBeReadOnly{T}"/> is a pair
///     of interfaces which helps to switch object state from read-only to mutable and back
///     to read-only. They should be implemented like following:<br/>
///
///     class ReadOnlyShard : ICanBeMutable&lt;MutableShard&gt; { }<br/>
///     class MutableShard : ICanBeReadOnly&lt;ReadOnlyShard&gt; { }
/// </remarks>
/// <typeparam name="TShard">A type of the <see cref="IModelShard"/></typeparam>
public interface ICanBeMutable<out TShard> : IModelShard
    where TShard : IModelShard
{
    /// <summary>
    ///     Returns a model shard which can be mutated
    /// </summary>
    /// <param name="features">A set of features which resulting mutable shard should include</param>
    /// <param name="modelChanges">A model changes</param>
    /// <returns>A mutable model shard</returns>
    TShard AsMutable(Features features, IWritableModelChanges modelChanges);
}
