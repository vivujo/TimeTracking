﻿using FS.TimeTracking.Abstractions.DTOs.Shared;
using FS.TimeTracking.Api.REST.Routing;
using FS.TimeTracking.Core.Interfaces.Application.Services.Shared;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace FS.TimeTracking.Api.REST.Controllers.Shared;

/// <inheritdoc cref="ITypeaheadService" />    
/// <seealso cref="ControllerBase" />
/// <seealso cref="ITypeaheadService" />
[V1ApiController]
[ExcludeFromCodeCoverage]
public class TypeaheadController : ControllerBase, ITypeaheadService
{
    private readonly ITypeaheadService _typeaheadService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeaheadController"/> class.
    /// </summary>
    /// <param name="typeaheadService">The typeahead service.</param>
    /// <autogeneratedoc />
    public TypeaheadController(ITypeaheadService typeaheadService)
        => _typeaheadService = typeaheadService;

    /// <inheritdoc />
    [HttpGet]
    public async Task<List<TypeaheadDto<Guid, string>>> GetCustomers(bool showHidden, CancellationToken cancellationToken = default)
        => await _typeaheadService.GetCustomers(showHidden, cancellationToken);

    /// <inheritdoc />
    [HttpGet]
    public async Task<List<TypeaheadDto<Guid, string>>> GetProjects(bool showHidden, CancellationToken cancellationToken = default)
        => await _typeaheadService.GetProjects(showHidden, cancellationToken);

    /// <inheritdoc />
    [HttpGet]
    public async Task<List<TypeaheadDto<Guid, string>>> GetOrders(bool showHidden, CancellationToken cancellationToken = default)
        => await _typeaheadService.GetOrders(showHidden, cancellationToken);

    /// <inheritdoc />
    [HttpGet]
    public async Task<List<TypeaheadDto<Guid, string>>> GetActivities(bool showHidden, CancellationToken cancellationToken = default)
        => await _typeaheadService.GetActivities(showHidden, cancellationToken);

    /// <inheritdoc />
    [HttpGet]
    public async Task<List<TypeaheadDto<string, string>>> GetTimezones(CancellationToken cancellationToken = default)
        => await _typeaheadService.GetTimezones(cancellationToken);
}