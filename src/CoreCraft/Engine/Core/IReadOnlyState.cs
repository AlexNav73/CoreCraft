namespace CoreCraft.Engine.Core;

/// <summary>
///     A model shard which can be mutated
/// </summary>
/// <remarks>
///     <see cref="IReadOnlyState{TMutableState}"/> and <see cref="IMutableState{TReadOnlyState}"/> is a pair
///     of interfaces which helps to switch object state from read-only to mutable state and back
///     to read-only. They should be implemented like following:<br/>
///
///     class ReadOnlyShard : IReadOnlyState&lt;MutableShard&gt; { }<br/>
///     class MutableShard : IMutableState&lt;ReadOnlyShard&gt; { }
/// </remarks>
/// <typeparam name="TMutableState">A mutable type from which a read-only object is created</typeparam>
public interface IReadOnlyState<out TMutableState>
{
    /// <summary>
    ///     Returns a model shard which can be mutated
    /// </summary>
    /// <param name="features">A collection of features to apply on the model shard</param>
    /// <returns>A mutable model shard</returns>
    TMutableState AsMutable(IEnumerable<IFeature> features);
}
