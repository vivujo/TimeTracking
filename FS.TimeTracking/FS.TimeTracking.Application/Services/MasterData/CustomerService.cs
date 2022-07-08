﻿using AutoMapper;
using FS.FilterExpressionCreator.Filters;
using FS.TimeTracking.Abstractions.DTOs.MasterData;
using FS.TimeTracking.Abstractions.DTOs.TimeTracking;
using FS.TimeTracking.Application.Services.Shared;
using FS.TimeTracking.Core.Extensions;
using FS.TimeTracking.Core.Interfaces.Application.Services.MasterData;
using FS.TimeTracking.Core.Interfaces.Repository.Services;
using FS.TimeTracking.Core.Models.Application.MasterData;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FS.TimeTracking.Application.Services.MasterData;

/// <inheritdoc cref="ICustomerService" />
public class CustomerService : CrudModelService<Customer, CustomerDto, CustomerGridDto>, ICustomerService
{
    /// <inheritdoc />
    public CustomerService(IRepository repository, IMapper mapper)
        : base(repository, mapper)
    { }

    /// <inheritdoc />
    public override async Task<List<CustomerGridDto>> GetGridFiltered(EntityFilter<TimeSheetDto> timeSheetFilter, EntityFilter<ProjectDto> projectFilter, EntityFilter<CustomerDto> customerFilter, EntityFilter<ActivityDto> activityFilter, EntityFilter<OrderDto> orderFilter, EntityFilter<HolidayDto> holidayFilter, CancellationToken cancellationToken = default)
    {
        var filter = FilterExtensions.CreateCustomerFilter(timeSheetFilter, projectFilter, customerFilter, activityFilter, orderFilter, holidayFilter);

        return await Repository
            .Get<Customer, CustomerGridDto>(
                where: filter,
                orderBy: o => o
                    .OrderBy(x => x.Hidden)
                    .ThenBy(x => x.Title)
                    .ThenBy(x => x.CompanyName)
                    .ThenBy(x => x.ContactName),
                cancellationToken: cancellationToken
            );
    }
}