using System.Runtime.Serialization;

namespace Navitski.Crystalized.Model.Engine.Exceptions;

public class ChangesFrameRegistrationException : Exception
{
    public ChangesFrameRegistrationException()
    {
    }

    public ChangesFrameRegistrationException(string? message)
        : base(message)
    {
    }

    public ChangesFrameRegistrationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected ChangesFrameRegistrationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
