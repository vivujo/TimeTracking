﻿using FS.FilterExpressionCreator.Filters;
using FS.TimeTracking.Application.Extensions;
using FS.TimeTracking.Shared.DTOs.Chart;
using FS.TimeTracking.Shared.DTOs.MasterData;
using FS.TimeTracking.Shared.DTOs.TimeTracking;
using FS.TimeTracking.Shared.Extensions;
using FS.TimeTracking.Shared.Interfaces.Application.Services.Chart;
using FS.TimeTracking.Shared.Interfaces.Application.Services.MasterData;
using FS.TimeTracking.Shared.Interfaces.Repository.Services;
using FS.TimeTracking.Shared.Models.Application.Chart;
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
                RatioTotalWorked = totalWorkedDays != 0 ? worked.WorkedDays / totalWorkedDays : 0,
                BudgetWorked = worked.WorkedBudget,
                Currency = settings.Currency,
            })
            .OrderBy(x => x.ActivityTitle)
            .ToList();

        return result;
    }

    private async Task<List<ActivityWorkTime>> GetWorkedTimesPerActivity(ChartFilter filter, CancellationToken cancellationToken)
    {
        var settings = await _settingService.GetSettings(cancellationToken);

        var workedTimesPerActivityAndOrder = await _repository
            .GetGrouped(
                groupBy: x => new { x.Activity.Id, x.Activity.Title, x.OrderId },
                @select: x => new ActivityWorkTime
                {
                    ActivityId = x.Key.Id,
                    ActivityTitle = x.Key.Title,
                    WorkedTime = TimeSpan.FromSeconds(x.Sum(f => (double)f.StartDateLocal.DiffSeconds(f.StartDateOffset, f.EndDateLocal))),
                    HourlyRate = x.Key.OrderId != null
                        ? x.Min(t => t.Order.HourlyRate)
                        : x.Min(t => t.Project.Customer.HourlyRate),
                },
                @where: filter.WorkedTimes.CreateFilter(),
                cancellationToken: cancellationToken
            );

        var workedTimesPerActivity = workedTimesPerActivityAndOrder
            .GroupBy(x => new { x.ActivityId, x.ActivityTitle })
            .Select(x => new ActivityWorkTime
            {
                ActivityId = x.Key.ActivityId,
                ActivityTitle = x.Key.ActivityTitle,
                WorkedTime = x.Sum(h => h.WorkedTime),
                HourlyRate = x.ToList().GetAverageHourlyRate(h => h.WorkedTime),
            })
            .ToList();

        foreach (var workTime in workedTimesPerActivity)
            workTime.WorkedDays = workTime.WorkedTime.TotalHours / settings.WorkHoursPerWorkday.TotalHours;

        return workedTimesPerActivity;
    }
}