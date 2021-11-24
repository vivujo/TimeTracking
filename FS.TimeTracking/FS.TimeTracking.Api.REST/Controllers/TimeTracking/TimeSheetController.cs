﻿using FS.FilterExpressionCreator.Filters;
using FS.TimeTracking.Api.REST.Controllers.Shared;
using FS.TimeTracking.Api.REST.Routing;
using FS.TimeTracking.Shared.DTOs.MasterData;
using FS.TimeTracking.Shared.DTOs.TimeTracking;
using FS.TimeTracking.Shared.Interfaces.Application.Services.TimeTracking;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FS.TimeTracking.Api.REST.Controllers.TimeTracking;

/// <inheritdoc cref="ITimeSheetService" />
/// <seealso cref="ControllerBase" />
/// <seealso cref="ITimeSheetService" />
[V1ApiController]
public class TimeSheetController : CrudModelController<TimeSheetDto, TimeSheetListDto>, ITimeSheetService
{
    private readonly ITimeSheetService _timeSheetService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSheetController"/> class.
    /// </summary>
    /// <param name="modelService">The model service.</param>
    public TimeSheetController(ITimeSheetService modelService)
        : base(modelService)
        => _timeSheetService = modelService;

    /// <inheritdoc />
    [HttpGet]
    public async Task<List<TimeSheetListDto>> ListFiltered([FromQuery] EntityFilter<TimeSheetDto> timeSheetFilter, [FromQuery] EntityFilter<ProjectDto> projectFilter, [FromQuery] EntityFilter<CustomerDto> customerFilter, [FromQuery] EntityFilter<ActivityDto> activityFilter, [FromQuery] EntityFilter<OrderDto> orderFilter, CancellationToken cancellationToken = default)
        => await _timeSheetService.ListFiltered(timeSheetFilter, projectFilter, customerFilter, activityFilter, orderFilter, cancellationToken);

    /// <inheritdoc />
    [HttpPut]
    public async Task<TimeSheetDto> StartSimilarTimeSheetEntry(Guid copyFromTimesheetId, DateTimeOffset startDateTime)
        => await _timeSheetService.StartSimilarTimeSheetEntry(copyFromTimesheetId, startDateTime);

    /// <inheritdoc />
    [HttpPut]
    public async Task<TimeSheetDto> StopTimeSheetEntry(Guid timesheetId, DateTimeOffset endDateTime)
        => await _timeSheetService.StopTimeSheetEntry(timesheetId, endDateTime);
}