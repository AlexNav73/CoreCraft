using System.Runtime.Serialization;

namespace Navitski.Crystalized.Model.Engine.Exceptions;

public class MissingRelationException : Exception
{
    public MissingRelationException()
    {
    }

    public MissingRelationException(string? message)
        : base(message)
    {
    }

    public MissingRelationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected MissingRelationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
