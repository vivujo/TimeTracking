﻿#if DEBUG
using FS.TimeTracking.Shared.Extensions;
using FS.TimeTracking.Shared.Interfaces.Application.Services;
using FS.TimeTracking.Shared.Interfaces.Repository.Services;
using FS.TimeTracking.Shared.Models.TimeTracking;
using System.Threading;
using System.Threading.Tasks;

namespace FS.TimeTracking.Application.Services
{
    /// <inheritdoc />
    public class DebugService : IDebugService
    {
        private readonly IRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugService"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <autogeneratedoc />
        public DebugService(IRepository repository)
            => _repository = repository;

        /// <inheritdoc />
        public async Task<object> TestMethod(CancellationToken cancellationToken = default)
        {
            var d1 = await _repository.Get((Order x) => x.StartDateLocal, cancellationToken: cancellationToken);
            var d2 = await _repository.Get((Order x) => x.StartDateLocal.ToUtc(120), cancellationToken: cancellationToken);
            var d3 = await _repository.Get((Order x) => x.StartDateLocal.ToUtc(x.StartDateOffset), cancellationToken: cancellationToken);
            var d4 = await _repository.Get((TimeSheet x) => x.EndDateLocal.ToUtc(x.EndDateOffset.Value), cancellationToken: cancellationToken);
            return Task.FromResult<object>(1);
        }
    }
}
#endif