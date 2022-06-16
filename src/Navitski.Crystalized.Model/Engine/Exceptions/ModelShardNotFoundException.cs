using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Navitski.Crystalized.Model.Engine.Exceptions;

/// <summary>
///     The exception occurred when retrieving model shard by the wrong type
/// </summary>
[ExcludeFromCodeCoverage]
public class ModelShardNotFoundException : Exception
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public ModelShardNotFoundException()
    {
    }

    /// <inheritdoc />
    public ModelShardNotFoundException(string? message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public ModelShardNotFoundException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected ModelShardNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
