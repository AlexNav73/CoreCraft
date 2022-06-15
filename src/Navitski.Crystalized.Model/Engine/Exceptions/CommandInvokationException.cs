using System.Runtime.Serialization;

namespace Navitski.Crystalized.Model.Engine.Exceptions;

/// <summary>
///     The exception occurred while invoking a command
/// </summary>
public class CommandInvokationException : Exception
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public CommandInvokationException()
    {
    }

    /// <inheritdoc />
    public CommandInvokationException(string? message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public CommandInvokationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected CommandInvokationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
