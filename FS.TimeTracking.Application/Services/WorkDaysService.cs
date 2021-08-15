﻿using FS.TimeTracking.Shared.Extensions;
using FS.TimeTracking.Shared.Interfaces.Application.Services;
using FS.TimeTracking.Shared.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FS.TimeTracking.Application.Services
{
    /// <inheritdoc />
    public class WorkDaysService : IWorkDaysService
    {
        private readonly TimeTrackingConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkDaysService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <autogeneratedoc />
        public WorkDaysService(TimeTrackingConfiguration configuration)
            => _configuration = configuration;

        /// <inheritdoc />
        public IEnumerable<DateTime> GetWorkDays(DateTime startDate, DateTime endDate)
            => GetWorkingDays(startDate.GetDays(endDate));

        /// <inheritdoc />
        public int GetWorkDaysCount(DateTime startDate, DateTime endDate)
            => (GetWorkDays(startDate, endDate)).Count();

        /// <inheritdoc />
        public IEnumerable<DateTime> GetWorkDaysOfMonth(int year, int month)
            => GetWorkingDays(new DateTime(year, month, 1).GetDaysOfMonth());

        /// <inheritdoc />
        public int GetWorkDaysCountOfMonth(int year, int month)
            => (GetWorkDaysOfMonth(year, month)).Count();

        /// <inheritdoc />
        public IEnumerable<DateTime> GetWorkDaysOfMonthTillDay(int year, int month, int day)
            => GetWorkingDays(new DateTime(year, month, day).GetDaysOfMonthTillDay());

        /// <inheritdoc />
        public int GetWorkDaysCountOfMonthTillDay(int year, int month, int day)
            => (GetWorkDaysOfMonthTillDay(year, month, day)).Count();

        private IEnumerable<DateTime> GetWorkingDays(IEnumerable<DateTime> dates)
        {
            return dates as DateTime[] ?? dates.ToArray()
                .Where(x => _configuration.WorkingDays.EmptyIfNull().Contains(x.DayOfWeek));
        }
    }
}