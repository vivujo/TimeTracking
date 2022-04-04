﻿using FS.TimeTracking.Abstractions.Models.Application.MasterData;
using System;
using System.ComponentModel.DataAnnotations;

namespace FS.TimeTracking.Abstractions.Models.Application.Chart;

/// <summary>
/// Work times per project.
/// </summary>
/// <autogeneratedoc />
internal class ProjectWorkTime : WorkTime
{
    /// <inheritdoc cref="Customer.Id"/>
    [Required]
    public Guid ProjectId { get; set; }

    /// <inheritdoc cref="Customer.Title"/>
    [Required]
    public string ProjectTitle { get; set; }

    /// <inheritdoc cref="Customer.Title"/>
    [Required]
    public string CustomerTitle { get; set; }
}