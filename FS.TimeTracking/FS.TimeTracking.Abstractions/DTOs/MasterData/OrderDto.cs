﻿using FS.FilterExpressionCreator.Abstractions.Attributes;
using FS.TimeTracking.Abstractions.Attributes;
using FS.TimeTracking.Abstractions.Models.Application.MasterData;
using FS.TimeTracking.Shared.Attributes;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace FS.TimeTracking.Abstractions.DTOs.MasterData;

/// <inheritdoc cref="Order"/>
[ValidationDescription]
[FilterEntity(Prefix = nameof(Order))]
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class OrderDto
{
    /// <inheritdoc cref="Order.Id"/>
    [Required]
    public Guid Id { get; set; }

    /// <inheritdoc cref="Order.Title"/>
    [Required]
    [StringLength(100)]
    public string Title { get; set; }

    /// <inheritdoc cref="Order.Description"/>
    public string Description { get; set; }

    /// <inheritdoc cref="Order.Number"/>
    [StringLength(100)]
    public string Number { get; set; }

    /// <inheritdoc cref="Order.CustomerId"/>
    [Required]
    [Filter(Visible = false)]
    public Guid CustomerId { get; set; }

    /// <inheritdoc cref="Order.StartDate"/>
    [Required]
    public DateTimeOffset StartDate { get; set; }

    /// <inheritdoc cref="Order.DueDate"/>
    [Required]
    [CompareTo(Models.Shared.ComparisonType.GreaterThanOrEqual, nameof(StartDate))]
    public DateTimeOffset DueDate { get; set; }

    /// <inheritdoc cref="Order.HourlyRate"/>
    [Required]
    [Range(0, double.PositiveInfinity)]
    public double HourlyRate { get; set; }

    /// <inheritdoc cref="Order.Budget"/>
    [Required]
    [Range(0, double.PositiveInfinity)]
    public double Budget { get; set; }

    /// <inheritdoc cref="Order.Comment"/>
    public string Comment { get; set; }

    /// <inheritdoc cref="Order.Hidden"/>
    [Required]
    public bool Hidden { get; set; }

    [JsonIgnore]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"{Title} ({Number})";
}