#if NETSTANDARD2_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Navitski.Crystalized.Model.Engine.Exceptions;

/// <summary>
///     The exception occurred while subscribing to a model shard property changes
/// </summary>
[ExcludeFromCodeCoverage]
public class InvalidPropertySubscriptionException : Exception
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public InvalidPropertySubscriptionException()
    {
    }

    /// <inheritdoc />
    public InvalidPropertySubscriptionException(string? message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public InvalidPropertySubscriptionException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected InvalidPropertySubscriptionException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
#endif
