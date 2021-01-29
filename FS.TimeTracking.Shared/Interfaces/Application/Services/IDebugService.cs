﻿using System.Threading;
using System.Threading.Tasks;

#if DEBUG
namespace FS.TimeTracking.Shared.Interfaces.Application.Services
{
    /// <summary>
    /// Interface IDebugService
    /// </summary>
    /// <autogeneratedoc />
    public interface IDebugService
    {
        /// <summary>
        /// Tests the method.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <autogeneratedoc />
        Task<object> TestMethod(CancellationToken cancellationToken = default);

        /// <summary>
        /// Seeds the data.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="truncateBeforeSeed">if set to <c>true</c> [truncate before seed].</param>
        /// <autogeneratedoc />
        Task SeedData(int amount = 10, bool truncateBeforeSeed = false);

        /// <summary>
        /// Truncates the data.
        /// </summary>
        /// <autogeneratedoc />
        Task TruncateData();
    }
}
#endif