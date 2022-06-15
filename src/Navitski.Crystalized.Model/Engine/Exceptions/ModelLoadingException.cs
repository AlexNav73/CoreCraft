using System.Runtime.Serialization;

namespace Navitski.Crystalized.Model.Engine.Exceptions;

/// <summary>
///     The exception occurred while loading the model
/// </summary>
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
