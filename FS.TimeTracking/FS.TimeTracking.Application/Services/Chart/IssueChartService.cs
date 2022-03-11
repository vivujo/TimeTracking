﻿using FS.FilterExpressionCreator.Abstractions.Extensions;
using FS.FilterExpressionCreator.Filters;
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
public class IssueChartService : IIssueChartService
{
    private readonly ISettingService _settingService;
    private readonly IRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="IssueChartService" /> class.
    /// </summary>
    /// <param name="settingService">The setting service.</param>
    /// <param name="repository">The repository.</param>
    /// <autogeneratedoc />
    public IssueChartService(ISettingService settingService, IRepository repository)
    {
        _settingService = settingService;
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<List<IssueWorkTimeDto>> GetWorkTimesPerIssue(EntityFilter<TimeSheetDto> timeSheetFilter, EntityFilter<ProjectDto> projectFilter, EntityFilter<CustomerDto> customerFilter, EntityFilter<ActivityDto> activityFilter, EntityFilter<OrderDto> orderFilter, EntityFilter<HolidayDto> holidayFilter, CancellationToken cancellationToken = default)
    {
        var settings = await _settingService.GetSettings(cancellationToken);
        var filter = ChartFilter.Create(timeSheetFilter, projectFilter, customerFilter, activityFilter, orderFilter, holidayFilter);
        var workedTimesPerIssue = await GetWorkedTimesPerIssue(filter, cancellationToken);

        var totalWorkedDays = workedTimesPerIssue.Sum(x => x.WorkedDays);

        var result = workedTimesPerIssue
            .Select(worked => new IssueWorkTimeDto
            {
                Issue = worked.Issue,
                CustomerTitle = worked.CustomerTitle,
                TimeWorked = worked.WorkedTime,
                DaysWorked = worked.WorkedDays,
                TotalWorkedPercentage = totalWorkedDays != 0 ? worked.WorkedDays / totalWorkedDays : 0,
                BudgetWorked = worked.WorkedBudget,
                Currency = settings.Company.Currency,
            })
            .OrderBy(x => x.CustomerTitle)
            .ThenBy(x => x.Issue)
            .ToList();

        return result;
    }

    private async Task<List<IssueWorkTime>> GetWorkedTimesPerIssue(ChartFilter filter, CancellationToken cancellationToken)
    {
        var settings = await _settingService.GetSettings(cancellationToken);

        var timeSheetsPerIssueAndOrder = await _repository
            .GetGrouped(
                groupBy: timeSheet => new { timeSheet.Issue, CustomerTitle = timeSheet.Project.Customer.Title, timeSheet.OrderId },
                select: timeSheets => new
                {
                    timeSheets.Key.Issue,
                    timeSheets.Key.CustomerTitle,
                    WorkedTime = TimeSpan.FromSeconds(timeSheets.Sum(f => (double)f.StartDateLocal.DiffSeconds(f.StartDateOffset, f.EndDateLocal))),
                    HourlyRate = timeSheets.Key.OrderId != null
                        ? timeSheets.Min(t => t.Order.HourlyRate)
                        : timeSheets.Min(t => t.Project.Customer.HourlyRate),
                },
                where: new[] { x => x.Issue != null, filter.WorkedTimes.CreateFilter() }.CombineWithConditionalAnd(),
                cancellationToken: cancellationToken
            );

        var workedTimesPerIssue = timeSheetsPerIssueAndOrder
            .GroupBy(timeSheet => new { timeSheet.Issue, timeSheet.CustomerTitle })
            .Select(timeSheets => new IssueWorkTime
            {
                Issue = timeSheets.Key.Issue,
                CustomerTitle = timeSheets.Key.CustomerTitle,
                WorkedTime = timeSheets.Sum(h => h.WorkedTime),
                WorkedBudget = timeSheets.Select(f => f.WorkedTime.TotalHours * f.HourlyRate).Sum(),
            })
            .ToList();

        foreach (var workTime in workedTimesPerIssue)
            workTime.WorkedDays = workTime.WorkedTime.TotalHours / settings.WorkHoursPerWorkday.TotalHours;

        return workedTimesPerIssue;
    }
}