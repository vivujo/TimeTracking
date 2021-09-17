﻿using FS.TimeTracking.Repository.DbFunctions;
using FS.TimeTracking.Shared.Interfaces.Models;
using FS.TimeTracking.Shared.Models.Configuration;
using FS.TimeTracking.Shared.Models.TimeTracking;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace FS.TimeTracking.Repository.DbContexts
{
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

            ConfigureCustomer(modelBuilder.Entity<Customer>());
            ConfigureProject(modelBuilder.Entity<Project>());
            ConfigureActivity(modelBuilder.Entity<Activity>());
            ConfigureOrder(modelBuilder.Entity<Order>());
            ConfigureTimeSheet(modelBuilder.Entity<TimeSheet>());

            RegisterDateTimeAsUtcConverter(modelBuilder);
        }

        private static void ConfigureCustomer(EntityTypeBuilder<Customer> customerBuilder)
        {
            customerBuilder
                .ToTable("Customers")
                .HasIndex(customer => new { customer.Title, customer.Hidden });
        }

        private static void ConfigureProject(EntityTypeBuilder<Project> projectBuilder)
        {
            projectBuilder
                .ToTable("Projects")
                .HasIndex(project => new { project.Title, project.Hidden });

            projectBuilder
                .HasOne(project => project.Customer)
                .WithMany(customer => customer.Projects)
                .HasForeignKey(project => project.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        }

        private static void ConfigureOrder(EntityTypeBuilder<Order> orderBuilder)
        {
            orderBuilder
                .ToTable("Orders")
                .HasIndex(project => new { project.Title, project.Hidden });

            orderBuilder
                .HasOne(project => project.Customer)
                .WithMany(customer => customer.Orders)
                .HasForeignKey(project => project.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        }

        private static void ConfigureActivity(EntityTypeBuilder<Activity> activityBuilder)
        {
            activityBuilder
                .ToTable("Activities")
                .HasIndex(activity => new { activity.Title, activity.Hidden });

            activityBuilder
                .HasOne(activity => activity.Project)
                .WithMany()
                .HasForeignKey(activity => activity.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureTimeSheet(EntityTypeBuilder<TimeSheet> timeSheetBuilder)
        {
            timeSheetBuilder
                .ToTable("TimeSheets");

            timeSheetBuilder
                .HasOne<Project>()
                .WithMany()
                .HasForeignKey(timeSheet => timeSheet.ProjectId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            timeSheetBuilder
                .HasOne<Activity>()
                .WithMany()
                .HasForeignKey(timeSheet => timeSheet.ActivityId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            timeSheetBuilder
                .HasOne<Order>()
                .WithMany()
                .HasForeignKey(timeSheet => timeSheet.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
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
}
