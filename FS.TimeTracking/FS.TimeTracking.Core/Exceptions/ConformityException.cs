﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FS.TimeTracking.Core.Exceptions;

/// <summary>
/// Exception for signalling conflicts when model is added or updated to database.
/// </summary>
public class ConformityException : Exception
{
    /// <summary>
    /// Gets detailed causes for the conformity violation.
    /// </summary>
    public IEnumerable<string> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConformityException"/> class.
    /// </summary>
    /// <param name="errors">A variable-length parameters list containing errors.</param>
    public ConformityException(params string[] errors)
        : base(GetExceptionMessage(errors))
        => Errors = errors;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConformityException"/> class.
    /// </summary>
    /// <param name="errors">A variable-length parameters list containing errors.</param>
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration", Justification = "Unable to resolve in code")]
    public ConformityException(IEnumerable<string> errors)
        : base(GetExceptionMessage(errors))
        => Errors = errors;

    private static string GetExceptionMessage(IEnumerable<string> errors)
        => string.Join(", ", errors);
}