﻿using FS.FilterExpressionCreator.Extensions;
using FS.FilterExpressionCreator.Filters;
using FS.TimeTracking.Core.Models.Filter;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace FS.TimeTracking.Application.Tests.Services;

[ExcludeFromCodeCoverage]
public static class FakeFilters
{
    public static TimeSheetFilterSet Empty()
        => new()
        {
            TimeSheetFilter = new(),
            ProjectFilter = new(),
            CustomerFilter = new(),
            ActivityFilter = new(),
            OrderFilter = new(),
            HolidayFilter = new(),
        };

    public static TimeSheetFilterSet Create(string timeSheetStartDate, string timeSheetEndDate)
    {
        timeSheetStartDate = AddTimeZoneOffsetIfMissing(timeSheetStartDate);
        timeSheetEndDate = AddTimeZoneOffsetIfMissing(timeSheetEndDate);

        var filters = Empty();
        filters.TimeSheetFilter.Add(x => x.StartDate, timeSheetStartDate);
        filters.TimeSheetFilter.Add(x => x.EndDate, timeSheetEndDate);
        return filters;
    }

    private static string AddTimeZoneOffsetIfMissing(string dateString)
    {
        const string offsetPattern = @"^(?<datetime>.+?)(?<offset>Z|[\+\-]\d{1,2}:\d{1,2})$";
        var hasOffsetPattern = Regex.IsMatch(dateString, offsetPattern);
        if (hasOffsetPattern)
            return dateString;

        var valueFilter = ValueFilter.Create(dateString);
        var date = valueFilter.Value.ConvertStringToDateTimeRange(DateTimeOffset.Now).Start;
        var dateOffset = FakeDateTime.DefaultTimezone.GetUtcOffset(date);
        var sign = dateOffset > TimeSpan.Zero ? "+" : "-";
        dateString += dateOffset.ToString($@"\{sign}hh\:mm");

        return dateString;
    }
}