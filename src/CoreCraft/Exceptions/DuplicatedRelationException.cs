using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CoreCraft.Exceptions;

/// <summary>
///     The exception occurred while adding duplicated parent-child relation
/// </summary>
[ExcludeFromCodeCoverage]
public class DuplicatedRelationException : Exception
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public DuplicatedRelationException()
    {
    }

    /// <inheritdoc />
    public DuplicatedRelationException(string? message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public DuplicatedRelationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected DuplicatedRelationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
