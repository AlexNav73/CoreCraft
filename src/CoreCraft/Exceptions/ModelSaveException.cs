using System.Diagnostics.CodeAnalysis;

namespace CoreCraft.Exceptions;

/// <summary>
///     The exception occurred while saving the model
/// </summary>
[ExcludeFromCodeCoverage]
public class ModelSaveException : Exception
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public ModelSaveException()
    {
    }

    /// <inheritdoc />
    public ModelSaveException(string? message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public ModelSaveException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
