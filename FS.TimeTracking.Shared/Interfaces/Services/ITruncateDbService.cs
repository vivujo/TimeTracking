﻿namespace FS.TimeTracking.Shared.Interfaces.Services
{
    /// <summary>
    /// Services to truncate whole database without removing the database itself
    /// </summary>
    /// <autogeneratedoc />
    public interface ITruncateDbService
    {
        /// <summary>
        /// Truncates the database without removing itself.
        /// </summary>
        void TruncateDatabase();
    }
}