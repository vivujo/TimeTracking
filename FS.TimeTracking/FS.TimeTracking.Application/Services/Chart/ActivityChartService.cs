﻿using FS.FilterExpressionCreator.Filters;
using FS.TimeTracking.Abstractions.DTOs.Chart;
using FS.TimeTracking.Abstractions.DTOs.MasterData;
using FS.TimeTracking.Abstractions.DTOs.TimeTracking;
using FS.TimeTracking.Abstractions.Interfaces.Application.Services.Chart;
using FS.TimeTracking.Abstractions.Interfaces.Application.Services.MasterData;
using FS.TimeTracking.Abstractions.Interfaces.Repository.Services;
using FS.TimeTracking.Abstractions.Models.Application.Chart;
using FS.TimeTracking.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FS.TimeTracking.Application.Services.Chart;

/// <inheritdoc />
public class ActivityChartService : IActivityChartService
{
    private readonly ISettingService _settingService;
    private readonly IRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActivityChartService" /> class.
    /// </summary>
    /// <param name="settingService">The setting service.</param>
    /// <param name="repository">The repository.</param>
    /// <autogeneratedoc />
    public ActivityChartService(ISettingService settingService, IRepository repository)
    {
        _settingService = settingService;
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<List<ActivityWorkTimeDto>> GetWorkTimesPerActivity(EntityFilter<TimeSheetDto> timeSheetFilter, EntityFilter<ProjectDto> projectFilter, EntityFilter<CustomerDto> customerFilter, EntityFilter<ActivityDto> activityFilter, EntityFilter<OrderDto> orderFilter, EntityFilter<HolidayDto> holidayFilter, CancellationToken cancellationToken = default)
    {
        var settings = await _settingService.GetSettings(cancellationToken);
        var filter = ChartFilter.Create(timeSheetFilter, projectFilter, customerFilter, activityFilter, orderFilter, holidayFilter);
        var workedTimesPerActivity = await GetWorkedTimesPerActivity(filter, cancellationToken);

        var totalWorkedDays = workedTimesPerActivity.Sum(x => x.WorkedDays);

        var result = workedTimesPerActivity
            .Select(worked => new ActivityWorkTimeDto
            {
                ActivityId = worked.ActivityId,
                ActivityTitle = worked.ActivityTitle,
                TimeWorked = worked.WorkedTime,
                DaysWorked = worked.WorkedDays,
                TotalWorkedPercentage = totalWorkedDays != 0 ? worked.WorkedDays / totalWorkedDays : 0,
                BudgetWorked = worked.WorkedBudget,
                Currency = settings.Company.Currency,
            })
            .OrderBy(x => x.ActivityTitle)
            .ToList();

        return result;
    }

    private async Task<List<ActivityWorkTime>> GetWorkedTimesPerActivity(ChartFilter filter, CancellationToken cancellationToken)
    {
        var settings = await _settingService.GetSettings(cancellationToken);

        var timeSheetsPerActivityAndOrder = await _repository
            .GetGrouped(
                groupBy: timeSheet => new { timeSheet.Activity.Id, timeSheet.Activity.Title, timeSheet.OrderId },
                select: timeSheets => new
                {
                    ActivityId = timeSheets.Key.Id,
                    ActivityTitle = timeSheets.Key.Title,
                    WorkedTime = TimeSpan.FromSeconds(timeSheets.Sum(f => (double)f.StartDateLocal.DiffSeconds(f.StartDateOffset, f.EndDateLocal))),
                    HourlyRate = timeSheets.Key.OrderId != null
                        ? timeSheets.Min(t => t.Order.HourlyRate)
                        : timeSheets.Min(t => t.Project.Customer.HourlyRate),
                },
                where: filter.WorkedTimes.CreateFilter(),
                cancellationToken: cancellationToken
            );

        var workedTimesPerActivity = timeSheetsPerActivityAndOrder
            .GroupBy(timeSheet => new { timeSheet.ActivityId, timeSheet.ActivityTitle })
            .Select(timeSheets => new ActivityWorkTime
            {
                ActivityId = timeSheets.Key.ActivityId,
                ActivityTitle = timeSheets.Key.ActivityTitle,
                WorkedTime = timeSheets.Sum(h => h.WorkedTime),
                WorkedBudget = timeSheets.Select(f => f.WorkedTime.TotalHours * f.HourlyRate).Sum(),
            })
            .ToList();

        foreach (var workTime in workedTimesPerActivity)
            workTime.WorkedDays = workTime.WorkedTime.TotalHours / settings.WorkHoursPerWorkday.TotalHours;

        return workedTimesPerActivity;
    }
}