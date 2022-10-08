﻿using AutoMapper;
using FS.TimeTracking.Core.Interfaces.Application.Services.Shared;
using FS.TimeTracking.Core.Interfaces.Models;
using FS.TimeTracking.Core.Interfaces.Repository.Services;
using FS.TimeTracking.Core.Models.Filter;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FS.TimeTracking.Application.Services.Shared;

/// <inheritdoc />
public abstract class CrudModelService<TModel, TDto, TGridDto> : ICrudModelService<TDto, TGridDto>
    where TModel : class, IIdEntityModel, new()
{
    /// <summary>
    /// The repository.
    /// </summary>
    protected readonly IRepository Repository;

    /// <summary>
    /// The mapper to convert models to DTOs and vice versa.
    /// </summary>
    protected readonly IMapper Mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CrudModelService{TModel, TDto, TQuery}"/> class.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="mapper">The mapper to convert models to DTOs and vice versa.</param>
    /// <autogeneratedoc />
    protected CrudModelService(IRepository repository, IMapper mapper)
    {
        Repository = repository;
        Mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<TDto> Get(Guid id, CancellationToken cancellationToken = default)
        => await Repository
            .FirstOrDefault<TModel, TDto>(
                where: model => model.Id == id,
                cancellationToken: cancellationToken
            );

    /// <inheritdoc />
    public abstract Task<List<TGridDto>> GetGridFiltered(TimeSheetFilterSet filters, CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public async Task<TGridDto> GetGridItem(Guid id, CancellationToken cancellationToken = default)
        => await Repository
            .FirstOrDefault<TModel, TGridDto>(
                where: model => model.Id == id,
                cancellationToken: cancellationToken
            );

    /// <inheritdoc />
    public async Task<TDto> Create(TDto dto)
    {
        var model = Mapper.Map<TModel>(dto);
        await CheckConformity(model);
        var result = await Repository.Add(model);
        await Repository.SaveChanges();
        return Mapper.Map<TDto>(result);
    }

    /// <inheritdoc />
    public async Task<TDto> Update(TDto dto)
    {
        var model = Mapper.Map<TModel>(dto);
        await CheckConformity(model);
        var result = Repository.Update(model);
        await Repository.SaveChanges();
        return Mapper.Map<TDto>(result);
    }

    /// <summary>
    /// Checks conformity of an entity on a deeper level than model validation can do before it's added or modified to database.
    /// </summary>
    /// <param name="model">The model to check.</param>
    protected virtual Task CheckConformity(TModel model)
        => Task.CompletedTask;

    /// <inheritdoc />
    public async Task<long> Delete(Guid id)
    {
        await Repository.Remove<TModel>(x => x.Id == id);
        return await Repository.SaveChanges();
    }
}