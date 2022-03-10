using System.Runtime.Serialization;

namespace Navitski.Crystalized.Model.Engine.Exceptions;

internal class ModelShardNotFoundException : Exception
{
    public ModelShardNotFoundException()
    {
    }

    public ModelShardNotFoundException(string? message)
        : base(message)
    {
    }

    public ModelShardNotFoundException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected ModelShardNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
