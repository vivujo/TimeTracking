﻿using FS.TimeTracking.Abstractions.DTOs.Chart;
using FS.TimeTracking.Core.Extensions;
using FS.TimeTracking.Core.Interfaces.Application.Services.Administration;
using FS.TimeTracking.Core.Interfaces.Application.Services.Chart;
using FS.TimeTracking.Core.Interfaces.Application.Services.Shared;
using FS.TimeTracking.Core.Interfaces.Repository.Services.Database;
using FS.TimeTracking.Core.Models.Application.Chart;
using FS.TimeTracking.Core.Models.Filter;
using Plainquire.Filter.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FS.TimeTracking.Application.Services.Chart;

/// <inheritdoc />
public class IssueChartService : IIssueChartApiService
{
    private readonly ISettingApiService _settingService;
    private readonly IFilterFactory _filterFactory;
    private readonly IDbRepository _dbRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="IssueChartService" /> class.
    /// </summary>
    /// <param name="settingService">The setting service.</param>
    /// <param name="filterFactory">The filter factory.</param>
    /// <param name="dbRepository">The repository.</param>
    /// <autogeneratedoc />
    public IssueChartService(ISettingApiService settingService, IFilterFactory filterFactory, IDbRepository dbRepository)
    {
        _settingService = settingService;
        _filterFactory = filterFactory;
        _dbRepository = dbRepository;
    }

    /// <inheritdoc />
    public async Task<List<IssueWorkTimeDto>> GetWorkTimesPerIssue(TimeSheetFilterSet filters, CancellationToken cancellationToken = default)
    {
        var settings = await _settingService.GetSettings(cancellationToken);
        var filter = await _filterFactory.CreateChartFilter(filters);
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

        var timeSheetsPerIssueAndOrder = await _dbRepository
            .GetGrouped(
                groupBy: timeSheet => new { timeSheet.Issue, CustomerTitle = timeSheet.Customer.Title, timeSheet.OrderId },
                select: timeSheets => new
                {
                    timeSheets.Key.Issue,
                    timeSheets.Key.CustomerTitle,
                    WorkedTime = TimeSpan.FromSeconds(timeSheets.Sum(f => (double)f.StartDateLocal.DiffSeconds(f.StartDateOffset, f.EndDateLocal, f.EndDateOffset))),
                    HourlyRate = timeSheets.Key.OrderId != null
                        ? timeSheets.Min(t => t.Order.HourlyRate)
                        : timeSheets.Min(t => t.Customer.HourlyRate),
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