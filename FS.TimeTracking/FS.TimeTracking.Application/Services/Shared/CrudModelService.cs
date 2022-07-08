﻿using AutoMapper;
using FS.FilterExpressionCreator.Filters;
using FS.TimeTracking.Abstractions.DTOs.MasterData;
using FS.TimeTracking.Abstractions.DTOs.TimeTracking;
using FS.TimeTracking.Core.Interfaces.Application.Services.Shared;
using FS.TimeTracking.Core.Interfaces.Models;
using FS.TimeTracking.Core.Interfaces.Repository.Services;
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
            .FirstOrDefault(
                select: (TModel model) => Mapper.Map<TDto>(model),
                where: model => model.Id == id,
                cancellationToken: cancellationToken
            );

    /// <inheritdoc />
    public abstract Task<List<TGridDto>> GetGridFiltered(EntityFilter<TimeSheetDto> timeSheetFilter, EntityFilter<ProjectDto> projectFilter, EntityFilter<CustomerDto> customerFilter, EntityFilter<ActivityDto> activityFilter, EntityFilter<OrderDto> orderFilter, EntityFilter<HolidayDto> holidayFilter, CancellationToken cancellationToken = default);

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
        var result = await Repository.Add(Mapper.Map<TModel>(dto));
        await Repository.SaveChanges();
        return Mapper.Map<TDto>(result);
    }

    /// <inheritdoc />
    public async Task<TDto> Update(TDto dto)
    {
        var result = Repository.Update(Mapper.Map<TModel>(dto));
        await Repository.SaveChanges();
        return Mapper.Map<TDto>(result);
    }

    /// <inheritdoc />
    public async Task<long> Delete(Guid id)
    {
        await Repository.Remove<TModel>(x => x.Id == id);
        return await Repository.SaveChanges();
    }
}