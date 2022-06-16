using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Navitski.Crystalized.Model.Engine.Exceptions;

/// <summary>
///     The exception occurred when retrieving not existing relation
/// </summary>
[ExcludeFromCodeCoverage]
public class MissingRelationException : Exception
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public MissingRelationException()
    {
    }

    /// <inheritdoc />
    public MissingRelationException(string? message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public MissingRelationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected MissingRelationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
