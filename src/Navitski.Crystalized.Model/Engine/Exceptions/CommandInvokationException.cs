using System.Runtime.Serialization;

namespace Navitski.Crystalized.Model.Engine.Exceptions;

public class CommandInvokationException : Exception
{
    public CommandInvokationException()
    {
    }

    public CommandInvokationException(string? message)
        : base(message)
    {
    }

    public CommandInvokationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected CommandInvokationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
