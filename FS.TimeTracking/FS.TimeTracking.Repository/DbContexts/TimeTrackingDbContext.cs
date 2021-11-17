﻿using FS.TimeTracking.Repository.DbFunctions;
using FS.TimeTracking.Shared.Interfaces.Models;
using FS.TimeTracking.Shared.Models.Configuration;
using FS.TimeTracking.Shared.Models.MasterData;
using FS.TimeTracking.Shared.Models.TimeTracking;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace FS.TimeTracking.Repository.DbContexts;

/// <inheritdoc />
public class TimeTrackingDbContext : DbContext
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly string _connectionString;
    private readonly DatabaseType _databaseType;
    private readonly EnvironmentConfiguration _environment;

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

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        LinqToDBForEFTools.Initialize();

        optionsBuilder
            .UseLoggerFactory(_loggerFactory)
            .EnableSensitiveDataLogging(_environment.IsDevelopment);

        var repositoryAssemblyName = typeof(TimeTrackingDbContext).Assembly.GetName().Name;
        var migrationAssembly = $"{repositoryAssemblyName}.{_databaseType}";

        switch (_databaseType)
        {
            case DatabaseType.SqLite:
                optionsBuilder.UseSqlite(_connectionString, o => o.MigrationsAssembly(migrationAssembly));
                optionsBuilder.RegisterSqLiteDateTimeFunctions();
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

    /// <inheritdoc />
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

    // https://stackoverflow.com/a/61243301/1271211
    private static void RegisterDateTimeAsUtcConverter(ModelBuilder modelBuilder)
    {
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>
        (
            v => v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
        );

        var properties = modelBuilder.Model
            .GetEntityTypes()
            .Where(x => x.ClrType.GetInterface(nameof(IEntityModel)) != null)
            .SelectMany(entityType => entityType.GetProperties())
            .Where(x => x.Name == nameof(IEntityModel.Created) || x.Name == nameof(IEntityModel.Modified))
            .ToList();

        foreach (var property in properties)
            property.SetValueConverter(dateTimeConverter);
    }
}