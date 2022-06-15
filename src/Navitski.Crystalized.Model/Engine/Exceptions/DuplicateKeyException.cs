using System.Runtime.Serialization;

namespace Navitski.Crystalized.Model.Engine.Exceptions;

/// <summary>
///     The exception occurred while inserting data with an existing key
/// </summary>
public class DuplicateKeyException : Exception
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public DuplicateKeyException()
    {
    }

    /// <inheritdoc />
    public DuplicateKeyException(string? message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public DuplicateKeyException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected DuplicateKeyException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
