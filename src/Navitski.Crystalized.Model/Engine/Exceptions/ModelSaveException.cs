using System.Runtime.Serialization;

namespace Navitski.Crystalized.Model.Engine.Exceptions;

internal class ModelSaveException : Exception
{
    public ModelSaveException()
    {
    }

    public ModelSaveException(string? message)
        : base(message)
    {
    }

    public ModelSaveException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected ModelSaveException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
