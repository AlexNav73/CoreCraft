using System.Diagnostics.CodeAnalysis;

namespace CoreCraft.Exceptions;

/// <summary>
///     The exception occurred when retrieving not existing relation
/// </summary>
[ExcludeFromCodeCoverage]
public class MissingRelationException : Exception
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public MissingRelationException()
    {
    }

    /// <inheritdoc />
    public MissingRelationException(string? message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public MissingRelationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
