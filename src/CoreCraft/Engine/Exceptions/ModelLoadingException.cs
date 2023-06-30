using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CoreCraft.Engine.Exceptions;

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

    /// <inheritdoc />
    protected ModelLoadingException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
