﻿using FS.TimeTracking.Core.Extensions;
using FS.TimeTracking.Core.Interfaces.Repository.Services.Database;
using FS.TimeTracking.Repository.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace FS.TimeTracking.Repository.Services;

/// <inheritdoc />
public class DbMigrationService : IDbMigrationService
{
    private readonly ILogger<TimeTrackingDbContext> _logger;
    private readonly TimeTrackingDbContext _dbContext;
    private readonly IDbTruncateService _dbTruncateService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbMigrationService"/> class.
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="dbContext">The DB context</param>
    /// <param name="dbTruncateService">The truncate service</param>
    /// <autogeneratedoc />
    public DbMigrationService(ILogger<TimeTrackingDbContext> logger, TimeTrackingDbContext dbContext, IDbTruncateService dbTruncateService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _dbTruncateService = dbTruncateService;
    }

    /// <inheritdoc />
    public async Task MigrateDatabase(bool truncateDatabase, CancellationToken cancellationToken = default)
    {
        if (truncateDatabase)
            _dbTruncateService.TruncateDatabase();

        var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken).ToListAsync();
        if (pendingMigrations.Count == 0)
            return;

        _logger.LogInformation("Apply migrations to database. Please be patient ...");
        foreach (var pendingMigration in pendingMigrations)
            _logger.LogInformation(pendingMigration);

        await _dbContext.Database.MigrateAsync(cancellationToken);

        _logger.LogInformation("Database migration finished.");
    }
}