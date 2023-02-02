﻿using AutoMapper;
using FS.FilterExpressionCreator.Abstractions.Models;
using FS.FilterExpressionCreator.Extensions;
using FS.FilterExpressionCreator.Filters;
using FS.TimeTracking.Abstractions.DTOs.Shared;
using FS.TimeTracking.Abstractions.DTOs.TimeTracking;
using FS.TimeTracking.Abstractions.Enums;
using FS.TimeTracking.Application.Services.Shared;
using FS.TimeTracking.Core.Constants;
using FS.TimeTracking.Core.Exceptions;
using FS.TimeTracking.Core.Extensions;
using FS.TimeTracking.Core.Interfaces.Application.Services.Administration;
using FS.TimeTracking.Core.Interfaces.Application.Services.Shared;
using FS.TimeTracking.Core.Interfaces.Application.Services.TimeTracking;
using FS.TimeTracking.Core.Interfaces.Repository.Services.Database;
using FS.TimeTracking.Core.Models.Application.MasterData;
using FS.TimeTracking.Core.Models.Application.TimeTracking;
using FS.TimeTracking.Core.Models.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FS.TimeTracking.Application.Services.TimeTracking;

/// <inheritdoc cref="ITimeSheetApiService" />
public class TimeSheetService : CrudModelService<TimeSheet, TimeSheetDto, TimeSheetGridDto>, ITimeSheetApiService
{
    private readonly IWorkdayService _workdayService;
    private readonly ISettingApiService _settingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSheetService"/> class.
    /// </summary>
    /// <param name="dbRepository">The repository.</param>
    /// <param name="mapper">The mapper.</param>
    /// <param name="filterFactory">The filter factory.</param>
    /// <param name="workdayService">The workday service.</param>
    /// <param name="settingService">The setting service.</param>
    /// <autogeneratedoc />
    public TimeSheetService(IDbRepository dbRepository, IMapper mapper, IFilterFactory filterFactory, IWorkdayService workdayService, ISettingApiService settingService)
        : base(dbRepository, mapper, filterFactory)
    {
        _workdayService = workdayService;
        _settingService = settingService;
    }

    /// <inheritdoc />
    public override async Task<List<TimeSheetGridDto>> GetGridFiltered(TimeSheetFilterSet filters, CancellationToken cancellationToken = default)
    {
        var filter = await FilterFactory.CreateTimeSheetFilter(filters);

        return await DbRepository
            .Get<TimeSheet, TimeSheetGridDto>(
                where: filter,
                orderBy: o => o.OrderByDescending(x => x.StartDateLocal),
                cancellationToken: cancellationToken
            );
    }

    /// <inheritdoc />
    public async Task<TimeSheetDto> StartSimilarTimeSheetEntry(Guid copyFromTimesheetId, DateTimeOffset startDateTime)
    {
        var timeSheet = await DbRepository.FirstOrDefault((TimeSheet x) => x, x => x.Id == copyFromTimesheetId);
        if (timeSheet == null)
            throw new InvalidOperationException($"Time sheet with ID {copyFromTimesheetId} does not exists.");

        var timeSheetCopy = Mapper.Map<TimeSheetDto>(timeSheet);
        timeSheetCopy.Id = Guid.NewGuid();
        timeSheetCopy.StartDate = startDateTime;
        timeSheetCopy.EndDate = null;
        return await Create(timeSheetCopy);
    }

    /// <inheritdoc />
    public async Task<TimeSheetDto> StopTimeSheetEntry(Guid timesheetId, DateTimeOffset endDateTime)
    {
        var timeSheet = await DbRepository.FirstOrDefault((TimeSheet x) => x, x => x.Id == timesheetId);
        if (timeSheet.EndDate != null)
            throw new InvalidOperationException($"Time sheet with ID {timesheetId} is already stopped.");

        timeSheet.EndDate = endDateTime;
        timeSheet = DbRepository.Update(timeSheet);
        await DbRepository.SaveChanges();
        return Mapper.Map<TimeSheetDto>(timeSheet);
    }

    /// <inheritdoc />
    public async Task<WorkedDaysInfoDto> GetWorkedDaysOverview(TimeSheetFilterSet filters, CancellationToken cancellationToken = default)
    {
        var filter = await FilterFactory.CreateTimeSheetFilter(filters);

        var dbMinMaxTotal = await DbRepository
            .GetGrouped(
                groupBy: (TimeSheet x) => 1,
                select: x => new
                {
                    MinStart = x.Min(timeSheet => timeSheet.StartDateLocal),
                    MaxEnd = x.Max(timeSheet => timeSheet.EndDateLocal),
                    TotalWorkedTime = TimeSpan.FromSeconds(x.Sum(f => (double)f.StartDateLocal.DiffSeconds(f.StartDateOffset, f.EndDateLocal, f.EndDateOffset)))
                },
                where: filter,
                cancellationToken: cancellationToken
            )
            .FirstOrDefaultAsync();

        var selectedPeriod = await filters.GetSelectedPeriod(true);
        selectedPeriod = await AlignPeriodToAvailableData(selectedPeriod, dbMinMaxTotal?.MinStart, dbMinMaxTotal?.MaxEnd);
        var workDays = await _workdayService.GetWorkdays(selectedPeriod, cancellationToken);

        var settings = await _settingService.GetSettings(cancellationToken);

        var totalWorkedTime = dbMinMaxTotal?.TotalWorkedTime ?? TimeSpan.Zero;

        var (lastWorkedTimes, aggregationUnit) = dbMinMaxTotal != null
            ? await GetLastWorkedTimes(dbMinMaxTotal.MinStart, dbMinMaxTotal.MaxEnd, filter, cancellationToken)
            : (new(), WorkdayAggregationUnit.Invalid);

        return new WorkedDaysInfoDto
        {
            PublicWorkdays = workDays.PublicWorkdays.Count,
            PersonalWorkdays = workDays.PersonalWorkdays.Count,
            WorkHoursPerWorkday = settings.WorkHoursPerWorkday,
            TotalTimeWorked = totalWorkedTime,
            LastWorkedTimes = lastWorkedTimes.Take(7).ToList(),
            LastWorkedTimesAggregationUnit = aggregationUnit
        };
    }

    /// <inheritdoc />
    protected override async Task CheckConformity(TimeSheet model)
    {
        await base.CheckConformity(model);

        var errors = new List<string>();

        var activityCustomerId = await DbRepository
            .FirstOrDefault(
                select: (Activity x) => x.CustomerId,
                where: x => x.Id == model.ActivityId
            );

        if (activityCustomerId != null && model.CustomerId != activityCustomerId)
            errors.Add("Customer of time sheet does not match customer of assigned activity.");

        if (model.ProjectId != null)
        {
            var projectCustomerId = await DbRepository
                .FirstOrDefault(
                    select: (Project x) => x.CustomerId,
                    where: x => x.Id == model.ProjectId
                );

            if (projectCustomerId != null && model.CustomerId != projectCustomerId)
                errors.Add("Customer of time sheet does not match customer of assigned project.");
        }

        if (model.OrderId != null)
        {
            var orderCustomerId = await DbRepository
                .FirstOrDefault(
                    select: (Order x) => x.CustomerId,
                    where: x => x.Id == model.OrderId
                );

            if (model.CustomerId != orderCustomerId)
                errors.Add("Customer of time sheet does not match customer of assigned order.");
        }

        if (errors.Any())
            throw new ConformityException(errors);
    }

    private static Task<Range<DateTimeOffset>> AlignPeriodToAvailableData(Range<DateTimeOffset> selectedPeriod, DateTime? minDate, DateTime? maxDate)
    {
        var now = DateTime.Now;
        var selectionHasStartValue = selectedPeriod.Start != DateOffset.MinDate;
        var selectionHasEndValue = selectedPeriod.End != DateOffset.MaxDate;
        var startDate = selectionHasStartValue ? selectedPeriod.Start : minDate ?? now;
        var endDate = selectionHasEndValue ? selectedPeriod.End : maxDate ?? now;
        selectedPeriod = new Range<DateTimeOffset>(startDate, endDate);
        return Task.FromResult(selectedPeriod);
    }

    private async Task<(List<WorkdayDto>, WorkdayAggregationUnit)> GetLastWorkedTimes(DateTime minDate, DateTime? maxDate, EntityFilter<TimeSheet> filter, CancellationToken cancellationToken)
    {
        var start = minDate;
        var end = maxDate ?? DateTime.Now;

        var selectedDays = (end - start).TotalDays;
        var moreThan7Months = selectedDays > (6 * 30 + 28);
        var moreThan7Weeks = selectedDays > 49;
        var moreThan7Days = selectedDays > 7;
        const int atLeast7Years = 6 * 365 + 2 * 366;
        const int atLeast7Months = 4 * 31 + 3 * 30 + 1;
        const int atLeast7Weeks = 8 * 7 + 1;
        const int atLeast7Days = 7 + 1;

        WorkdayAggregationUnit aggregationUnit;
        if (moreThan7Months)
            aggregationUnit = WorkdayAggregationUnit.Year;
        else if (moreThan7Weeks)
            aggregationUnit = WorkdayAggregationUnit.Month;
        else if (moreThan7Days)
            aggregationUnit = WorkdayAggregationUnit.Week;
        else
            aggregationUnit = WorkdayAggregationUnit.Day;

        start = aggregationUnit switch
        {
            WorkdayAggregationUnit.Year => end.AddDays(-atLeast7Years),
            WorkdayAggregationUnit.Month => end.AddDays(-atLeast7Months),
            WorkdayAggregationUnit.Week => end.AddDays(-atLeast7Weeks),
            WorkdayAggregationUnit.Day => end.AddDays(-atLeast7Days),
            _ => throw new ArgumentOutOfRangeException(nameof(aggregationUnit))
        };

        filter = filter.Replace(x => x.StartDate, $"<{end:O}");
        filter = filter.Replace(x => x.EndDate, $">={start:O},ISNULL");

        var workedTimePerDay = await DbRepository
            .GetGrouped(
                groupBy: (TimeSheet x) => x.StartDateLocal.Date,
                select: x => new WorkdayDto
                {
                    Date = x.Min(timeSheet => timeSheet.StartDateLocal),
                    TimeWorked = TimeSpan.FromSeconds(x.Sum(f => (double)f.StartDateLocal.DiffSeconds(f.StartDateOffset, f.EndDateLocal, f.EndDateOffset)))
                },
                where: filter,
                cancellationToken: cancellationToken
            );

        var lastWorkedTimes = aggregationUnit switch
        {
            WorkdayAggregationUnit.Year => GroupAndFillMissing(workedTimePerDay, start, end, x => x.StartOfYear()),
            WorkdayAggregationUnit.Month => GroupAndFillMissing(workedTimePerDay, start, end, x => x.StartOfMonth()),
            WorkdayAggregationUnit.Week => GroupAndFillMissing(workedTimePerDay, start, end, x => x.StartOfWeek()),
            WorkdayAggregationUnit.Day => GroupAndFillMissing(workedTimePerDay, start, end, x => x.StartOfDay()),
            _ => throw new ArgumentOutOfRangeException(nameof(aggregationUnit))
        };

        return (lastWorkedTimes.Take(7).ToList(), aggregationUnit);
    }

    private static List<WorkdayDto> GroupAndFillMissing(IEnumerable<WorkdayDto> workedTimePerDay, DateTime start, DateTime end, Func<DateTime, DateTime> groupKeySelector)
    {
        var requiredDates = start.GetDays(end)
            .GroupBy(groupKeySelector)
            .TakeLast(7)
            .Select(x => x.Key)
            .ToList();

        var existingGroups = workedTimePerDay
            .GroupBy(workday => groupKeySelector(workday.Date))
            .Select(group => new WorkdayDto { Date = group.Key, TimeWorked = group.Sum(workday => workday.TimeWorked) })
            .ToList();

        return requiredDates
            .OuterJoin(
                existingGroups,
                requiredDate => requiredDate,
                workday => workday.Date,
                (requiredDate, workday) => workday ?? new() { Date = requiredDate, TimeWorked = TimeSpan.Zero }
            )
            .ToList();
    }
}