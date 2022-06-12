namespace Navitski.Crystalized.Model.Engine.Commands;

/// <summary>
///     A command parameter wrapper
/// </summary>
/// <remarks>
///     Wraps a value to provide automatic validation of input parameters
/// </remarks>
public interface ICommandParameter
{
    /// <summary>
    ///     Parameter name
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Is parameter initialized
    /// </summary>
    bool IsInitialized { get; set; }
}

/// <inheritdoc cref="ICommandParameter"/>
public interface ICommandParameter<T> : ICommandParameter
{
    /// <summary>
    ///     A value
    /// </summary>
    T Value { get; }

    /// <summary>
    ///     Sets a parameter value
    /// </summary>
    /// <param name="value">A new parameter value</param>
    void Set(T value);
}
