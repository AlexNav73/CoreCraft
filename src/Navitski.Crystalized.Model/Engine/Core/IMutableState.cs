namespace Navitski.Crystalized.Model.Engine.Core;

/// <summary>
///     An interface which switches an object state from mutable to a read-only
/// </summary>
/// <remarks>
///     <see cref="IReadOnlyState{TMutableState}"/> and <see cref="IMutableState{TReadOnlyState}"/> is a pair
///     of interfaces which helps to switch object state from read-only to mutable and back
///     to read-only. They should be implemented like following:<br/>
///
///     class ReadOnlyShard : IReadOnlyState&lt;MutableShard&gt; { }<br/>
///     class MutableShard : IMutableState&lt;ReadOnlyShard&gt; { }
/// </remarks>
/// <typeparam name="TReadOnlyState">A read-only type from which a mutable object is created</typeparam>
public interface IMutableState<out TReadOnlyState>
{
    /// <summary>
    ///     Switches object state from mutable to read-only
    /// </summary>
    /// <returns>Read-only object</returns>
    TReadOnlyState AsReadOnly();
}
