﻿using FS.TimeTracking.Abstractions.Interfaces.DTOs;
using FS.TimeTracking.Core.Interfaces.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace FS.TimeTracking.Core.Interfaces.Application.Services.Shared;

/// <summary>
/// Service to query / check users privileges.
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Gets a value indicating whether the authorization is disabled.
    /// </summary>
    bool AuthorizationDisabled { get; }

    /// <summary>
    /// Gets the current user / identity.
    /// </summary>
    ClaimsPrincipal CurrentUser { get; }

    /// <summary>
    /// Gets the current user identifier.
    /// </summary>
    /// <value>
    /// The identifier of the current user.
    /// </value>
    /// <autogeneratedoc />
    Guid CurrentUserId { get; }

    /// <summary>
    /// Indicates whether the current user is allowed to view data of other users.
    /// </summary>
    bool CanViewForeignData { get; }

    /// <summary>
    /// Indicates whether the current user is allowed to manage entities according to the user with ID <paramref name="userId"/>
    /// </summary>
    bool CanView(Guid userId);

    /// <summary>
    /// Indicates whether the current user is allowed to manage entities according to the user with ID <paramref name="userId"/>
    /// </summary>
    bool CanManage(Guid userId);

    /// <summary>
    /// Indicates whether the current user is allowed to manage <typeparamref name="TEntity"/> with the given <paramref name="id"/>
    /// </summary>
    Task<bool> CanManage<TEntity>(Guid id) where TEntity : class, IIdEntityModel, IUserRelatedModel;

    /// <summary>
    /// Indicates whether the current user is allowed to manage <typeparamref name="TEntity"/>
    /// </summary>
    Task<bool> CanManage<TDto, TEntity>(TDto dto)
        where TDto : class, IIdEntityDto, IUserRelatedDto
        where TEntity : class, IIdEntityModel, IUserRelatedModel;

    /// <summary>
    /// Sets user related fields of a DTO.
    /// </summary>
    /// <typeparam name="TDto">Generic type parameter.</typeparam>
    /// <param name="entity">The DTO to work on.</param>
    /// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
    Task SetAuthorizationRelatedProperties<TDto>(TDto entity, CancellationToken cancellationToken = default) where TDto : class, IManageableDto;

    /// <summary>
    /// Sets user related fields of a DTO.
    /// </summary>
    /// <typeparam name="TDto">Generic type parameter.</typeparam>
    /// <param name="entities">The DTOs to work on.</param>
    /// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
    Task SetAuthorizationRelatedProperties<TDto>(List<TDto> entities, CancellationToken cancellationToken = default) where TDto : class, IManageableDto;
}