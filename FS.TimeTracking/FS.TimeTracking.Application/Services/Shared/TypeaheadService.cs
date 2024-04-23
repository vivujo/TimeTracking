﻿using FS.TimeTracking.Abstractions.DTOs.Shared;
using FS.TimeTracking.Application.Extensions;
using FS.TimeTracking.Core.Interfaces.Application.Services.Administration;
using FS.TimeTracking.Core.Interfaces.Application.Services.Shared;
using FS.TimeTracking.Core.Interfaces.Repository.Services.Database;
using FS.TimeTracking.Core.Models.Application.MasterData;
using FS.TimeTracking.Core.Models.Filter;
using Plainquire.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FS.TimeTracking.Application.Services.Shared;

/// <inheritdoc />
public class TypeaheadService : ITypeaheadApiService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserService _userService;
    private readonly IDbRepository _dbRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeaheadService"/> class.
    /// </summary>
    /// <param name="authorizationService">The authorization service.</param>
    /// <param name="userService">The user service.</param>
    /// <param name="dbRepository">The repository.</param>
    /// <autogeneratedoc />
    public TypeaheadService(IAuthorizationService authorizationService, IUserService userService, IDbRepository dbRepository)
    {
        _authorizationService = authorizationService;
        _userService = userService;
        _dbRepository = dbRepository;
    }

    /// <inheritdoc />
    public async Task<List<TypeaheadDto<Guid, string>>> GetCustomers(bool showHidden, CancellationToken cancellationToken = default)
    {
        var filter = new EntityFilter<Customer>()
            .SetHidden(x => x.Hidden, showHidden)
            .RestrictToCustomers(x => x.Id, _authorizationService.RestrictToCustomerIds);

        return await _dbRepository
            .Get(
                select: (Customer x) => TypeaheadDto.Create(x.Id, x.Title, x.Hidden),
                where: filter,
                orderBy: o => o.OrderBy(x => x.Hidden).ThenBy(x => x.Title),
                cancellationToken: cancellationToken
            );
    }

    /// <inheritdoc />
    public async Task<List<TypeaheadDto<Guid, string>>> GetProjects(bool showHidden, CancellationToken cancellationToken = default)
    {
        var filter = new EntityFilter<Project>()
            .SetHidden(x => x.Hidden, showHidden)
            .RestrictToCustomers(x => x.CustomerId, _authorizationService.RestrictToCustomerIds);

        return await _dbRepository
            .Get(
                select: (Project x) => new TypeaheadDto<Guid, string>
                {
                    Id = x.Id,
                    Value = x.Customer != null ? $"{x.Title} ({x.Customer.Title})" : x.Title,
                    Hidden = x.Hidden,
                    Extended = new { x.CustomerId }
                },
                where: filter,
                orderBy: o => o.OrderBy(x => x.Hidden).ThenBy(x => x.Title).ThenBy(x => x.Customer.Title),
                cancellationToken: cancellationToken
            );
    }

    /// <inheritdoc />
    public async Task<List<TypeaheadDto<Guid, string>>> GetOrders(bool showHidden, CancellationToken cancellationToken = default)
    {
        var filter = new EntityFilter<Order>()
            .SetHidden(x => x.Hidden, showHidden)
            .RestrictToCustomers(x => x.CustomerId, _authorizationService.RestrictToCustomerIds);

        return await _dbRepository
            .Get(
                select: (Order x) => new TypeaheadDto<Guid, string>
                {
                    Id = x.Id,
                    Value = x.Number != null ? $"{x.Title} ({x.Number})" : x.Title,
                    Hidden = x.Hidden,
                    Extended = new
                    {
                        IsActive = x.StartDateLocal.Date <= DateTime.UtcNow.Date && x.DueDateLocal >= DateTimeOffset.UtcNow.Date,
                        x.CustomerId
                    }
                },
                where: filter,
                orderBy: o => o.OrderBy(x => x.Hidden).ThenBy(x => x.Title),
                cancellationToken: cancellationToken
            );
    }

    /// <inheritdoc />
    public async Task<List<TypeaheadDto<Guid, string>>> GetActivities(bool showHidden, CancellationToken cancellationToken = default)
    {
        var filter = new EntityFilter<Activity>()
            .SetHidden(x => x.Hidden, showHidden)
            .RestrictToCustomers(x => x.CustomerId, _authorizationService.RestrictToCustomerIds);

        return await _dbRepository
            .Get(
                select: (Activity x) => new TypeaheadDto<Guid, string>
                {
                    Id = x.Id,
                    Value = x.Project != null ? $"{x.Title} ({x.Project.Title})" : x.Customer != null ? $"{x.Title} ({x.Customer.Title})" : x.Title,
                    Hidden = x.Hidden,
                    Extended = new { x.CustomerId, x.ProjectId, }
                },
                where: filter,
                orderBy: o => o.OrderBy(x => x.Hidden).ThenBy(x => x.Title),
                cancellationToken: cancellationToken
            );
    }

    /// <inheritdoc />
    public async Task<List<TypeaheadDto<Guid, string>>> GetUsers(bool showHidden, CancellationToken cancellationToken = default)
    {
        var filter = new TimeSheetFilterSet();
        if (!showHidden)
            filter.UserFilter.Add(x => x.Enabled, true);

        var users = await _userService.GetFiltered(filter, cancellationToken);

        return users
            .Select(user => new TypeaheadDto<Guid, string>
            {
                Id = user.Id,
                Value = user.Username,
                Hidden = !user.Enabled
            })
            .ToList();
    }

    /// <inheritdoc />
    public Task<List<TypeaheadDto<string, string>>> GetTimezones(CancellationToken cancellationToken = default)
        => Task.FromResult(TimeZoneInfo.GetSystemTimeZones().Select(x => TypeaheadDto.Create(x.Id, x.DisplayName)).ToList());
}