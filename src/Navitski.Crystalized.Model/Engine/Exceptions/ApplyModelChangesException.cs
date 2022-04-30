using System.Runtime.Serialization;

namespace Navitski.Crystalized.Model.Engine.Exceptions;

public class ApplyModelChangesException : Exception
{
    public ApplyModelChangesException()
    {
    }

    public ApplyModelChangesException(string? message)
        : base(message)
    {
    }

    public ApplyModelChangesException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected ApplyModelChangesException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
