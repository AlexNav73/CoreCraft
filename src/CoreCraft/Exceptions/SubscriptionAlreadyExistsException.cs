using System.Diagnostics.CodeAnalysis;

namespace CoreCraft.Exceptions;

/// <summary>
///     The exception occurred when subscribing to the model changes the same handler multiple times
/// </summary>
[ExcludeFromCodeCoverage]
public class SubscriptionAlreadyExistsException : Exception
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public SubscriptionAlreadyExistsException()
    {
    }

    /// <inheritdoc />
    public SubscriptionAlreadyExistsException(string? message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public SubscriptionAlreadyExistsException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
