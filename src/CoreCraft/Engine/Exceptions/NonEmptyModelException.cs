using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CoreCraft.Engine.Exceptions;

/// <summary>
///     The exception occurred while loading data to the non-empty model
/// </summary>
/// <remarks>
///     The model should be empty because the loaded data can interfere with
///     already existing data inside the model and the resulting model state
///     will differ from the save model state. When some changes will happen
///     with old data in the model and this changes will be saved this can cause
///     updating or removal of the non-existent data from the storage.
/// </remarks>
[ExcludeFromCodeCoverage]
public class NonEmptyModelException : Exception
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public NonEmptyModelException()
    {
    }

    /// <inheritdoc />
    public NonEmptyModelException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public NonEmptyModelException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected NonEmptyModelException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
