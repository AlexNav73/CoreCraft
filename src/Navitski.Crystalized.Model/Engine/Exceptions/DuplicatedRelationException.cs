using System.Runtime.Serialization;

namespace Navitski.Crystalized.Model.Engine.Exceptions;

public class DuplicatedRelationException : Exception
{
    public DuplicatedRelationException()
    {
    }

    public DuplicatedRelationException(string? message)
        : base(message)
    {
    }

    public DuplicatedRelationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected DuplicatedRelationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
