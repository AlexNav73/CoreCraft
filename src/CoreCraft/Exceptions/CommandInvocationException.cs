﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CoreCraft.Exceptions;

/// <summary>
///     The exception occurred while invoking a command
/// </summary>
[ExcludeFromCodeCoverage]
public class CommandInvocationException : Exception
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public CommandInvocationException()
    {
    }

    /// <inheritdoc />
    public CommandInvocationException(string? message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public CommandInvocationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected CommandInvocationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
