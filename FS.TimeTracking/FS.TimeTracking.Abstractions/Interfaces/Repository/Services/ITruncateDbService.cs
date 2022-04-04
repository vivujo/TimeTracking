﻿namespace FS.TimeTracking.Abstractions.Interfaces.Repository.Services;

/// <summary>
/// Services to truncate whole database without removing the database itself
/// </summary>
public interface ITruncateDbService
{
    /// <summary>
    /// Truncates the database without removing itself.
    /// </summary>
    void TruncateDatabase();
}