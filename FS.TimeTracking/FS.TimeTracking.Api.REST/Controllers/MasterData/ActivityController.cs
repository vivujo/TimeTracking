﻿using FS.TimeTracking.Abstractions.DTOs.MasterData;
using FS.TimeTracking.Api.REST.Controllers.Shared;
using FS.TimeTracking.Api.REST.Routing;
using FS.TimeTracking.Core.Interfaces.Application.Services.MasterData;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace FS.TimeTracking.Api.REST.Controllers.MasterData;

/// <inheritdoc cref="IActivityService" />
/// <seealso cref="ControllerBase" />
/// <seealso cref="IActivityService" />
[V1ApiController]
[ExcludeFromCodeCoverage]
public class ActivityController : CrudModelController<ActivityDto, ActivityGridDto>, IActivityService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ActivityController"/> class.
    /// </summary>
    /// <param name="modelService">The model service.</param>
    public ActivityController(IActivityService modelService)
        : base(modelService)
    {
    }
}