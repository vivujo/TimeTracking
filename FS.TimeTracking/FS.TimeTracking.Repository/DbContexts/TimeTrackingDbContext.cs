﻿using FS.TimeTracking.Core.Interfaces.Models;
using FS.TimeTracking.Core.Models.Application.MasterData;
using FS.TimeTracking.Core.Models.Application.TimeTracking;
using FS.TimeTracking.Core.Models.Configuration;
using FS.TimeTracking.Repository.DbFunctions;
using LinqToDB.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;

namespace FS.TimeTracking.Repository.DbContexts;

/// <summary>
/// Application specific database context.
/// Implements <see cref="Microsoft.EntityFrameworkCore.DbContext" />
/// </summary>
/// <seealso cref="Microsoft.EntityFrameworkCore.DbContext" />
public class TimeTrackingDbContext : DbContext
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly DatabaseType _databaseType;
    private readonly EnvironmentConfiguration _environment;
    private readonly string _connectionString;
    private SqliteConnection _keepAliveConnection;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeTrackingDbContext"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="environment">The environment.</param>
    /// <autogeneratedoc />
    public TimeTrackingDbContext(ILoggerFactory loggerFactory, IOptions<TimeTrackingConfiguration> configuration, EnvironmentConfiguration environment)
    {
        _loggerFactory = loggerFactory;
        _connectionString = configuration.Value.Database.ConnectionString;
        _databaseType = configuration.Value.Database.Type;
        _environment = environment;
    }

    /// <summary>
    /// <para>
    /// Override this method to configure the database (and other options) to be used for this context.
    /// This method is called for each instance of the context that is created.
    /// The base implementation does nothing.
    /// </para>
    /// <para>
    /// In situations where an instance of <see cref="T:Microsoft.EntityFrameworkCore.DbContextOptions" /> may or may not have been passed
    /// to the constructor, you can use <see cref="P:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.IsConfigured" /> to determine if
    /// the options have already been set, and skip some or all of the logic in
    /// <see cref="M:Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)" />.
    /// </para>
    /// </summary>
    /// <param name="optionsBuilder">A builder used to create or modify options for this context. Databases (and other extensions)
    /// typically define extension methods on this object that allow you to configure the context.</param>
    /// <exception cref="System.ArgumentOutOfRangeException">null - Configured database type is unsupported</exception>
    /// <remarks>See <see href="https://aka.ms/efcore-docs-dbcontext">DbContext lifetime, configuration, and initialization</see>
    /// for more information.</remarks>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        LinqToDBForEFTools.Initialize();

        optionsBuilder
            .UseLoggerFactory(_loggerFactory)
            .EnableSensitiveDataLogging(_environment.IsDevelopment);

        var repositoryAssemblyName = typeof(TimeTrackingDbContext).Assembly.GetName().Name;
        var migrationAssembly = _databaseType switch
        {
            DatabaseType.InMemory => $"{repositoryAssemblyName}.{DatabaseType.Sqlite}",
            _ => $"{repositoryAssemblyName}.{_databaseType}",
        };

        switch (_databaseType)
        {
            case DatabaseType.InMemory:
                _keepAliveConnection?.Close();
                _keepAliveConnection?.Dispose();
                _keepAliveConnection = new SqliteConnection(_connectionString);
                _keepAliveConnection.Open();
                optionsBuilder.UseSqlite(_connectionString, o => o.MigrationsAssembly(migrationAssembly));
                optionsBuilder.RegisterSqliteDateTimeFunctions();
                break;
            case DatabaseType.Sqlite:
                var connectionStringBuilder = new SqliteConnectionStringBuilder(_connectionString);
                var databaseDirectory = Path.GetDirectoryName(connectionStringBuilder.DataSource);
                if (!string.IsNullOrWhiteSpace(databaseDirectory))
                    Directory.CreateDirectory(databaseDirectory);
                optionsBuilder.UseSqlite(_connectionString, o => o.MigrationsAssembly(migrationAssembly));
                optionsBuilder.RegisterSqliteDateTimeFunctions();
                break;
            case DatabaseType.SqlServer:
                optionsBuilder.UseSqlServer(_connectionString, o => o.MigrationsAssembly(migrationAssembly));
                break;
            case DatabaseType.PostgreSql:
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                optionsBuilder.UseNpgsql(_connectionString, o => o.MigrationsAssembly(migrationAssembly));
                break;
            case DatabaseType.MySql:
                var serverVersion = ServerVersion.AutoDetect(_connectionString);
                optionsBuilder.UseMySql(_connectionString, serverVersion, o => o.MigrationsAssembly(migrationAssembly));
                break;
            default:
                throw new ArgumentOutOfRangeException(null, "Configured database type is unsupported");
        }
    }

    /// <summary>
    /// Releases the allocated resources for this context.
    /// </summary>
    /// <remarks>
    /// See <see href="https://aka.ms/efcore-docs-dbcontext">DbContext lifetime, configuration, and initialization</see>
    /// for more information.
    /// </remarks>
    /// <autogeneratedoc />
    public override void Dispose()
    {
        base.Dispose();
        _keepAliveConnection?.Close();
        _keepAliveConnection?.Dispose();
    }

    /// <summary>
    /// Override this method to further configure the model that was discovered by convention from the entity types
    /// exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties on your derived context. The resulting model may be cached
    /// and re-used for subsequent instances of your derived context.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context. Databases (and other extensions) typically
    /// define extension methods on this object that allow you to configure aspects of the model that are specific
    /// to a given database.</param>
    /// <remarks><para>
    /// If a model is explicitly set on the options for this context (via <see cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)" />)
    /// then this method will not be run.
    /// </para>
    /// <para>
    /// See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information.
    /// </para></remarks>
    /// <autogeneratedoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.RegisterDateTimeFunctions(_databaseType);

        ConfigureSetting(modelBuilder.Entity<Setting>());
        ConfigureHoliday(modelBuilder.Entity<Holiday>());
        ConfigureCustomer(modelBuilder.Entity<Customer>());
        ConfigureProject(modelBuilder.Entity<Project>());
        ConfigureActivity(modelBuilder.Entity<Activity>());
        ConfigureOrder(modelBuilder.Entity<Order>());
        ConfigureTimeSheet(modelBuilder.Entity<TimeSheet>());

        RegisterDateTimeAsUtcConverter(modelBuilder);
        RegisterSqliteGuidToStringConverter(modelBuilder);
    }

    private static void ConfigureSetting(EntityTypeBuilder<Setting> settingsBuilder)
    {
        settingsBuilder
            .ToTable("Settings")
            .HasKey(x => x.Key);
    }

    private void ConfigureHoliday(EntityTypeBuilder<Holiday> holidayBuilder)
    {
        holidayBuilder
            .ToTable("Holidays");

        if (_databaseType == DatabaseType.PostgreSql)
        {
            holidayBuilder
                .Property(x => x.StartDateLocal)
                .HasColumnType("timestamp");

            holidayBuilder
                .Property(x => x.EndDateLocal)
                .HasColumnType("timestamp");
        }
    }

    private static void ConfigureCustomer(EntityTypeBuilder<Customer> customerBuilder)
    {
        customerBuilder
            .ToTable("Customers");
    }

    private static void ConfigureProject(EntityTypeBuilder<Project> projectBuilder)
    {
        projectBuilder
            .ToTable("Projects");

        projectBuilder
            .HasOne(project => project.Customer)
            .WithMany(customer => customer.Projects)
            .HasForeignKey(project => project.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }

    private void ConfigureOrder(EntityTypeBuilder<Order> orderBuilder)
    {
        orderBuilder
            .ToTable("Orders");

        orderBuilder
            .HasOne(project => project.Customer)
            .WithMany(customer => customer.Orders)
            .HasForeignKey(project => project.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        if (_databaseType == DatabaseType.PostgreSql)
        {
            orderBuilder
                .Property(x => x.StartDateLocal)
                .HasColumnType("timestamp");

            orderBuilder
                .Property(x => x.DueDateLocal)
                .HasColumnType("timestamp");
        }
    }

    private static void ConfigureActivity(EntityTypeBuilder<Activity> activityBuilder)
    {
        activityBuilder
            .ToTable("Activities");

        activityBuilder
            .HasOne(activity => activity.Project)
            .WithMany()
            .HasForeignKey(activity => activity.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private void ConfigureTimeSheet(EntityTypeBuilder<TimeSheet> timeSheetBuilder)
    {

        timeSheetBuilder
            .ToTable("TimeSheets");

        timeSheetBuilder
            .Property(x => x.StartDateLocal)
            .HasConversion(GetDateTimeCutSecondsConverter());

        timeSheetBuilder
            .Property(x => x.EndDateLocal)
            .HasConversion(GetNullableDateTimeCutSecondsConverter());

        timeSheetBuilder
            .HasOne(x => x.Project)
            .WithMany()
            .HasForeignKey(timeSheet => timeSheet.ProjectId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        timeSheetBuilder
            .HasOne(x => x.Activity)
            .WithMany()
            .HasForeignKey(timeSheet => timeSheet.ActivityId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        timeSheetBuilder
            .HasOne(x => x.Order)
            .WithMany()
            .HasForeignKey(timeSheet => timeSheet.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        if (_databaseType == DatabaseType.PostgreSql)
        {
            timeSheetBuilder
                .Property(x => x.StartDateLocal)
                .HasColumnType("timestamp");

            timeSheetBuilder
                .Property(x => x.EndDateLocal)
                .HasColumnType("timestamp");
        }
    }

    private static ValueConverter<DateTime, DateTime> GetDateTimeCutSecondsConverter()
    {
        var ticksPerMinute = TimeSpan.FromMinutes(1).Ticks;
        return new ValueConverter<DateTime, DateTime>
        (
            v => new DateTime(v.Ticks / ticksPerMinute * ticksPerMinute),
            v => v
        );
    }

    private static ValueConverter<DateTime?, DateTime?> GetNullableDateTimeCutSecondsConverter()
    {
        var ticksPerMinute = TimeSpan.FromMinutes(1).Ticks;
        return new ValueConverter<DateTime?, DateTime?>
        (
            v => v.HasValue ? new DateTime(v.Value.Ticks / ticksPerMinute * ticksPerMinute) : v,
            v => v
        );
    }

    // https://stackoverflow.com/a/61243301/1271211
    private static void RegisterDateTimeAsUtcConverter(ModelBuilder modelBuilder)
    {
        var dateTimeAsUtcConverter = new ValueConverter<DateTime, DateTime>
        (
            v => v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
        );

        var properties = modelBuilder.Model
            .GetEntityTypes()
            .Where(entityType => entityType.ClrType.GetInterface(nameof(IEntityModel)) != null)
            .SelectMany(entityType => entityType.GetProperties())
            .Where(x => x.Name == nameof(IEntityModel.Created) || x.Name == nameof(IEntityModel.Modified))
            .ToList();

        foreach (var property in properties)
            property.SetValueConverter(dateTimeAsUtcConverter);
    }

    // https://stackoverflow.com/a/61243301/1271211
    private void RegisterSqliteGuidToStringConverter(ModelBuilder modelBuilder)
    {
        if (_databaseType != DatabaseType.InMemory && _databaseType != DatabaseType.Sqlite)
            return;

        var guidToStringConverter = new ValueConverter<Guid, string>
        (
            v => v.ToString("D"),
            v => Guid.Parse(v)
        );

        var guidProperties = modelBuilder.Model
            .GetEntityTypes()
            .Where(x => x.ClrType.GetInterface(nameof(IEntityModel)) != null)
            .SelectMany(entityType => entityType.GetProperties())
            .Where(x => x.ClrType == typeof(Guid))
            .ToList();

        foreach (var property in guidProperties)
            property.SetValueConverter(guidToStringConverter);

        var nullableGuidProperties = modelBuilder.Model
            .GetEntityTypes()
            .Where(x => x.ClrType.GetInterface(nameof(IEntityModel)) != null)
            .SelectMany(entityType => entityType.GetProperties())
            .Where(x => x.ClrType == typeof(Guid?))
            .ToList();

        var nullableGuidToStringConverter = new ValueConverter<Guid?, string>
        (
            v => v != null ? v.Value.ToString("D") : null,
            v => v != null ? Guid.Parse(v) : null
        );

        foreach (var property in nullableGuidProperties)
            property.SetValueConverter(nullableGuidToStringConverter);
    }
}