using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CoreCraft.Engine.Exceptions;

/// <summary>
///     The exception occurred when changes produced in a wrong order (or the changes are incompatible
///     with each other like modifying already removed entity)
/// </summary>
[ExcludeFromCodeCoverage]
public class InvalidChangeSequenceException : Exception
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public InvalidChangeSequenceException(object previousChange, object nextChange)
    {
        PreviousChange = previousChange;
        NextChange = nextChange;
    }

    /// <inheritdoc />
    public InvalidChangeSequenceException(object previousChange, object nextChange, string? message)
        : base(message)
    {
        PreviousChange = previousChange;
        NextChange = nextChange;
    }

    /// <inheritdoc />
    public InvalidChangeSequenceException(object previousChange, object nextChange, string? message, Exception? innerException)
        : base(message, innerException)
    {
        PreviousChange = previousChange;
        NextChange = nextChange;
    }

    /// <inheritdoc />
    protected InvalidChangeSequenceException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        PreviousChange = null!;
        NextChange = null!;
    }

    /// <summary>
    ///     The previous change
    /// </summary>
    public object PreviousChange { get; set; }

    /// <summary>
    ///     The next change that is incompatible with <see cref="PreviousChange"/>
    /// </summary>
    public object NextChange { get; set; }
}
