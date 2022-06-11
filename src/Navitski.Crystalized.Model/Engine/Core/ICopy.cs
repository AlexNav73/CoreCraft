namespace Navitski.Crystalized.Model.Engine.Core;

/// <summary>
///     Marks a type which can be copied
/// </summary>
/// <typeparam name="T">A type which will be copied</typeparam>
public interface ICopy<out T>
{
    /// <summary>
    ///     Copies an instance
    /// </summary>
    /// <returns>A new copy of an instance</returns>
    T Copy();
}
