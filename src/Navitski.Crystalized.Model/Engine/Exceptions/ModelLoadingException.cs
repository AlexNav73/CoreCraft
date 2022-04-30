using System.Runtime.Serialization;

namespace Navitski.Crystalized.Model.Engine.Exceptions;

public class ModelLoadingException : Exception
{
    public ModelLoadingException()
    {
    }

    public ModelLoadingException(string? message)
        : base(message)
    {
    }

    public ModelLoadingException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected ModelLoadingException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
