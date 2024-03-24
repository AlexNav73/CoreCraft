using System.Diagnostics.CodeAnalysis;

namespace CoreCraft.Exceptions;

/// <summary>
///     The exception occurred while loading the model
/// </summary>
[ExcludeFromCodeCoverage]
public class ModelLoadingException : Exception
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public ModelLoadingException()
    {
    }

    /// <inheritdoc />
    public ModelLoadingException(string? message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public ModelLoadingException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
