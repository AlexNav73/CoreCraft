using System.Diagnostics.CodeAnalysis;

namespace CoreCraft.Exceptions;

/// <summary>
///     The exception occurred while applying changes to the model
/// </summary>
[ExcludeFromCodeCoverage]
public class ApplyModelChangesException : Exception
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public ApplyModelChangesException()
    {
    }

    /// <inheritdoc />
    public ApplyModelChangesException(string? message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public ApplyModelChangesException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
