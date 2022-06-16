using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Navitski.Crystalized.Model.Engine.Exceptions;

/// <summary>
///     The exception occurred while applying changes to the model
/// </summary>
[ExcludeFromCodeCoverage]
public class ApplyModelChangesException : Exception
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public ApplyModelChangesException()
    {
    }

    /// <inheritdoc />
    public ApplyModelChangesException(string? message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public ApplyModelChangesException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected ApplyModelChangesException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
