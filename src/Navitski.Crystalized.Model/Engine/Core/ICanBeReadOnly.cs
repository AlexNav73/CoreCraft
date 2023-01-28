namespace Navitski.Crystalized.Model.Engine.Core;

/// <summary>
///     An interface which switches an object state from mutable to a read-only
/// </summary>
/// <remarks>
///     <see cref="ICanBeMutable{TShard}"/> and <see cref="ICanBeReadOnly{T}"/> is a pair
///     of interfaces which helps to switch object state from read-only to mutable and back
///     to read-only. They should be implemented like following:<br/>
///
///     class ReadOnlyShard : ICanBeMutable&lt;MutableShard&gt; { }<br/>
///     class MutableShard : ICanBeReadOnly&lt;ReadOnlyShard&gt; { }
/// </remarks>
/// <typeparam name="T">A read-only type from which a mutable object is created</typeparam>
public interface ICanBeReadOnly<out T>
{
    /// <summary>
    ///     Switches object state from mutable to read-only
    /// </summary>
    /// <returns>Read-only object</returns>
    T AsReadOnly();
}
