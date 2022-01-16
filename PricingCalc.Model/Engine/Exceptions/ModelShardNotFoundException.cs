using System.Runtime.Serialization;

namespace PricingCalc.Model.Engine.Exceptions;

internal class ModelShardNotFoundException : Exception
{
    public ModelShardNotFoundException()
    {
    }

    public ModelShardNotFoundException(string? modelShardName)
        : base($"Model shard [{modelShardName}] not found")
    {
    }

    public ModelShardNotFoundException(string? modelShardName, Exception? innerException)
        : base($"Model shard [{modelShardName}] not found", innerException)
    {
    }

    protected ModelShardNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
