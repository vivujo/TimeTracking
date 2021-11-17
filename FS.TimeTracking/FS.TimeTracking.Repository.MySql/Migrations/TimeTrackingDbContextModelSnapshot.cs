﻿// <auto-generated />
using System;
using FS.TimeTracking.Repository.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FS.TimeTracking.Repository.MySql.Migrations
{
    [DbContext(typeof(TimeTrackingDbContext))]
    partial class TimeTrackingDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("FS.TimeTracking.Shared.Models.MasterData.Activity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Comment")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("Hidden")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("datetime(6)");

                    b.Property<Guid?>("ProjectId")
                        .HasColumnType("char(36)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.ToTable("Activities", (string)null);
                });

            modelBuilder.Entity("FS.TimeTracking.Shared.Models.MasterData.Customer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("City")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Comment")
                        .HasColumnType("longtext");

                    b.Property<string>("CompanyName")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("ContactName")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Country")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Department")
                        .HasColumnType("longtext");

                    b.Property<bool>("Hidden")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Number")
                        .HasColumnType("longtext");

                    b.Property<string>("Street")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("ZipCode")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Customers", (string)null);
                });

            modelBuilder.Entity("FS.TimeTracking.Shared.Models.MasterData.Holiday", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("EndDateLocal")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("EndDateOffset")
                        .HasColumnType("int");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("StartDateLocal")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("StartDateOffset")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Holidays", (string)null);
                });

            modelBuilder.Entity("FS.TimeTracking.Shared.Models.MasterData.Order", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<double>("Budget")
                        .HasColumnType("double");

                    b.Property<string>("Comment")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime(6)");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("char(36)");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("DueDateLocal")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("DueDateOffset")
                        .HasColumnType("int");

                    b.Property<bool>("Hidden")
                        .HasColumnType("tinyint(1)");

                    b.Property<double>("HourlyRate")
                        .HasColumnType("double");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Number")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<DateTime>("StartDateLocal")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("StartDateOffset")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.ToTable("Orders", (string)null);
                });

            modelBuilder.Entity("FS.TimeTracking.Shared.Models.MasterData.Project", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Comment")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime(6)");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("char(36)");

                    b.Property<bool>("Hidden")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.ToTable("Projects", (string)null);
                });

            modelBuilder.Entity("FS.TimeTracking.Shared.Models.MasterData.Setting", b =>
                {
                    b.Property<string>("Key")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Description")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Key");

                    b.ToTable("Settings", (string)null);
                });

            modelBuilder.Entity("FS.TimeTracking.Shared.Models.TimeTracking.TimeSheet", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid>("ActivityId")
                        .HasColumnType("char(36)");

                    b.Property<bool>("Billable")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Comment")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("EndDateLocal")
                        .HasColumnType("datetime(6)");

                    b.Property<int?>("EndDateOffset")
                        .HasColumnType("int");

                    b.Property<string>("Issue")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("datetime(6)");

                    b.Property<Guid?>("OrderId")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("ProjectId")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("StartDateLocal")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("StartDateOffset")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ActivityId");

                    b.HasIndex("OrderId");

                    b.HasIndex("ProjectId");

                    b.ToTable("TimeSheets", (string)null);
                });

            modelBuilder.Entity("FS.TimeTracking.Shared.Models.MasterData.Activity", b =>
                {
                    b.HasOne("FS.TimeTracking.Shared.Models.MasterData.Project", "Project")
                        .WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Project");
                });

            modelBuilder.Entity("FS.TimeTracking.Shared.Models.MasterData.Order", b =>
                {
                    b.HasOne("FS.TimeTracking.Shared.Models.MasterData.Customer", "Customer")
                        .WithMany("Orders")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("FS.TimeTracking.Shared.Models.MasterData.Project", b =>
                {
                    b.HasOne("FS.TimeTracking.Shared.Models.MasterData.Customer", "Customer")
                        .WithMany("Projects")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("FS.TimeTracking.Shared.Models.TimeTracking.TimeSheet", b =>
                {
                    b.HasOne("FS.TimeTracking.Shared.Models.MasterData.Activity", "Activity")
                        .WithMany()
                        .HasForeignKey("ActivityId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("FS.TimeTracking.Shared.Models.MasterData.Order", "Order")
                        .WithMany()
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("FS.TimeTracking.Shared.Models.MasterData.Project", "Project")
                        .WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Activity");

                    b.Navigation("Order");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("FS.TimeTracking.Shared.Models.MasterData.Customer", b =>
                {
                    b.Navigation("Orders");

                    b.Navigation("Projects");
                });
#pragma warning restore 612, 618
        }
    }
}
