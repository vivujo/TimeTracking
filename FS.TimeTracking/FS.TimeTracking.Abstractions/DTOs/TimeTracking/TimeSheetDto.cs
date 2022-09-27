﻿using FS.FilterExpressionCreator.Abstractions.Attributes;
using FS.TimeTracking.Abstractions.Attributes;
using FS.TimeTracking.Abstractions.DTOs.MasterData;
using FS.TimeTracking.Abstractions.Enums;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace FS.TimeTracking.Abstractions.DTOs.TimeTracking;

/// <summary>
/// Time sheet position.
/// </summary>
[ValidationDescription]
[FilterEntity(Prefix = "TimeSheet")]
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class TimeSheetDto
{
    /// <summary>
    /// The unique identifier of the entity.
    /// </summary>
    [Required]
    public Guid Id { get; set; }

    /// <summary>
    /// The identifier to the related <see cref="ProjectDto"/>.
    /// </summary>
    [Required]
    [Filter(Visible = false)]
    public Guid ProjectId { get; set; }

    /// <summary>
    /// The identifier to the related <see cref="ActivityDto"/>.
    /// </summary>
    [Required]
    [Filter(Visible = false)]
    public Guid ActivityId { get; set; }

    /// <summary>
    /// The identifier to the related <see cref="OrderDto"/>.
    /// </summary>
    [Filter(Visible = false)]
    public Guid? OrderId { get; set; }

    /// <summary>
    /// The related issue/ticket/... .
    /// </summary>
    public string Issue { get; set; }

    /// <summary>
    /// The start date.
    /// </summary>
    [Required]
    public DateTimeOffset StartDate { get; set; }

    /// <summary>
    /// The end date.
    /// </summary>
    [CompareTo(ComparisonType.GreaterThan, nameof(StartDate))]
    public DateTimeOffset? EndDate { get; set; }

    /// <summary>
    /// Indicates whether this item is billable.
    /// </summary>
    [Required]
    public bool Billable { get; set; }

    /// <summary>
    /// Comment for this item.
    /// </summary>
    public string Comment { get; set; }

    [JsonIgnore]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"{StartDate:dd.MM.yyyy HH:mm} - {EndDate:dd.MM.yyyy HH:mm}";
}