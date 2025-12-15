using CoreCraft.ChangesTracking;

namespace CoreCraft.Core;

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
    /// 
    /// </summary>
    /// <returns></returns>
    IChangesFrame Create();

    /// <summary>
    ///     Returns a model shard which can be mutated
    /// </summary>
    /// <returns>A mutable model shard</returns>
    TMutableState AsRunCommandModel(IMutableModelChanges changes);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="changes"></param>
    /// <returns></returns>
    TMutableState AsLoadModel(IMutableModelChanges changes);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    TMutableState AsApplyModel();
}
