﻿using FS.TimeTracking.Abstractions.DTOs.MasterData;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FS.TimeTracking.Abstractions.DTOs.TimeTracking;

/// <inheritdoc cref="TimeSheetDto"/>
[ExcludeFromCodeCoverage]
[System.Diagnostics.DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class TimeSheetGridDto
{
    /// <inheritdoc cref="TimeSheetDto.Id"/>
    [Required]
    public Guid Id { get; set; }

    /// <inheritdoc cref="TimeSheetDto.StartDate"/>
    [Required]
    public DateTimeOffset StartDate { get; set; }

    /// <inheritdoc cref="TimeSheetDto.EndDate"/>
    public DateTimeOffset? EndDate { get; set; }

    /// <summary>
    /// Total working time for this sheet.
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <inheritdoc cref="TimeSheetDto.Comment"/>
    public string Comment { get; set; }

    /// <inheritdoc cref="TimeSheetDto.Issue"/>
    public string Issue { get; set; }

    /// <inheritdoc cref="CustomerDto.Title"/>
    public string CustomerTitle { get; set; }

    /// <inheritdoc cref="ProjectDto.Title"/>
    public string ProjectTitle { get; set; }

    /// <inheritdoc cref="ActivityDto.Title"/>
    public string ActivityTitle { get; set; }

    /// <inheritdoc cref="OrderDto.Title"/>
    public string OrderTitle { get; set; }

    /// <inheritdoc cref="TimeSheetDto.Billable"/>
    [Required]
    public bool Billable { get; set; }

    [JsonIgnore]
    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay =>
        $"{StartDate:dd.MM.yyyy HH:mm} - {EndDate:dd.MM.yyyy HH:mm}"
        + (CustomerTitle != null ? $", {CustomerTitle}" : string.Empty)
        + (ProjectTitle != null ? $", {ProjectTitle}" : string.Empty)
        + (ActivityTitle != null ? $", {ActivityTitle}" : string.Empty);
}