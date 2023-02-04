﻿using FS.TimeTracking.Core.Interfaces.Application.Services.Shared;
using Microsoft.AspNetCore.Http;

namespace FS.TimeTracking.Application.Services.Shared;

/// <inheritdoc />
public class AuthorizationService : IAuthorizationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationService"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <autogeneratedoc />
    public AuthorizationService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc />
    public bool IsCurrentUserInRole(string role)
        => _httpContextAccessor.HttpContext?.User.IsInRole(role) == true;
}