﻿using AutoMapper;
using FS.FilterExpressionCreator.Abstractions.Models;
using FS.FilterExpressionCreator.Extensions;
using FS.FilterExpressionCreator.Filters;
using FS.TimeTracking.Abstractions.DTOs.MasterData;
using FS.TimeTracking.Abstractions.DTOs.Shared;
using FS.TimeTracking.Abstractions.DTOs.TimeTracking;
using FS.TimeTracking.Abstractions.Enums;
using FS.TimeTracking.Abstractions.Extensions;
using FS.TimeTracking.Abstractions.Interfaces.Application.Services.MasterData;
using FS.TimeTracking.Abstractions.Interfaces.Application.Services.Shared;
using FS.TimeTracking.Abstractions.Interfaces.Application.Services.TimeTracking;
using FS.TimeTracking.Abstractions.Interfaces.Repository.Services;
using FS.TimeTracking.Abstractions.Models.Application.TimeTracking;
using FS.TimeTracking.Application.Services.Shared;
using FS.TimeTracking.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FS.TimeTracking.Application.Services.TimeTracking;

/// <inheritdoc cref="ITimeSheetService" />
public class TimeSheetService : CrudModelService<TimeSheet, TimeSheetDto, TimeSheetGridDto>, ITimeSheetService
{
    private readonly IWorkdayService _workdayService;
    private readonly ISettingService _settingService;

    /// <inheritdoc />
    public TimeSheetService(IRepository repository, IMapper mapper, IWorkdayService workdayService, ISettingService settingService)
        : base(repository, mapper)
    {
        _workdayService = workdayService;
        _settingService = settingService;
    }

    /// <inheritdoc />
    public override async Task<List<TimeSheetGridDto>> GetGridFiltered(EntityFilter<TimeSheetDto> timeSheetFilter, EntityFilter<ProjectDto> projectFilter, EntityFilter<CustomerDto> customerFilter, EntityFilter<ActivityDto> activityFilter, EntityFilter<OrderDto> orderFilter, EntityFilter<HolidayDto> holidayFilter, CancellationToken cancellationToken = default)
    {
        var filter = FilterExtensions.CreateTimeSheetFilter(timeSheetFilter, projectFilter, customerFilter, activityFilter, orderFilter, holidayFilter);

        return await Repository
            .Get<TimeSheet, TimeSheetGridDto>(
                where: filter,
                orderBy: o => o.OrderByDescending(x => x.StartDateLocal),
                cancellationToken: cancellationToken
            );
    }

    /// <inheritdoc />
    public async Task<TimeSheetDto> StartSimilarTimeSheetEntry(Guid copyFromTimesheetId, DateTimeOffset startDateTime)
    {
        var timeSheet = await Repository.FirstOrDefault((TimeSheet x) => x, x => x.Id == copyFromTimesheetId);
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
        var timeSheet = await Repository.FirstOrDefault((TimeSheet x) => x, x => x.Id == timesheetId);
        if (timeSheet.EndDate != null)
            throw new InvalidOperationException($"Time sheet with ID {timesheetId} is already stopped.");

        timeSheet.EndDate = endDateTime;
        timeSheet = Repository.Update(timeSheet);
        await Repository.SaveChanges();
        return Mapper.Map<TimeSheetDto>(timeSheet);
    }

    /// <inheritdoc />
    public async Task<WorkedDaysInfoDto> GetWorkedDaysOverview(EntityFilter<TimeSheetDto> timeSheetFilter, EntityFilter<ProjectDto> projectFilter, EntityFilter<CustomerDto> customerFilter, EntityFilter<ActivityDto> activityFilter, EntityFilter<OrderDto> orderFilter, EntityFilter<HolidayDto> holidayFilter, CancellationToken cancellationToken = default)
    {
        var filter = FilterExtensions.CreateTimeSheetFilter(timeSheetFilter, projectFilter, customerFilter, activityFilter, orderFilter, holidayFilter);

        var dbMinMaxTotal = await Repository
            .GetGrouped(
                groupBy: (TimeSheet x) => 1,
                select: x => new
                {
                    Start = x.Min(timeSheet => timeSheet.StartDateLocal),
                    End = x.Max(timeSheet => timeSheet.EndDateLocal),
                    TotalWorkedTime = TimeSpan.FromSeconds(x.Sum(f => (double)f.StartDateLocal.DiffSeconds(f.StartDateOffset, f.EndDateLocal)))
                },
                where: filter,
                cancellationToken: cancellationToken
            )
            .AsEnumerableAsync()
            .FirstOrDefaultAsync();

        var now = DateTime.Now;
        var selectedPeriod = FilterExtensions.GetSelectedPeriod(timeSheetFilter, true);
        var selectionHasStartValue = selectedPeriod.Start != DateTimeOffset.MinValue;
        var selectionHasEndValue = selectedPeriod.End != DateTimeOffset.MaxValue;
        var startDate = selectionHasStartValue ? selectedPeriod.Start : dbMinMaxTotal?.Start ?? now;
        var endDate = selectionHasEndValue ? selectedPeriod.End : dbMinMaxTotal?.End ?? now;
        selectedPeriod = new Range<DateTimeOffset>(startDate, endDate);
        var workDays = await _workdayService.GetWorkdays(selectedPeriod, cancellationToken);

        var settings = await _settingService.GetSettings(cancellationToken);

        var totalWorkedTime = dbMinMaxTotal?.TotalWorkedTime ?? TimeSpan.Zero;

        var (lastWorkedTimes, aggregationUnit) = dbMinMaxTotal != null
            ? await GetLastWorkedTimes(dbMinMaxTotal.Start, dbMinMaxTotal.End, filter, cancellationToken)
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

        var workedTimePerDay = await Repository
            .GetGrouped(
                groupBy: (TimeSheet x) => x.StartDateLocal.Date,
                select: x => new WorkdayDto
                {
                    Date = x.Min(timeSheet => timeSheet.StartDateLocal),
                    TimeWorked = TimeSpan.FromSeconds(x.Sum(f => (double)f.StartDateLocal.DiffSeconds(f.StartDateOffset, f.EndDateLocal)))
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