﻿using FS.TimeTracking.Shared.Interfaces.Application.Converters;
using FS.TimeTracking.Shared.Interfaces.Application.Services;
using FS.TimeTracking.Shared.Interfaces.Models;
using FS.TimeTracking.Shared.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FS.TimeTracking.Application.Services
{
    /// <inheritdoc />
    public abstract class CrudModelService<TModel, TDto, TListDto> : ICrudModelService<TDto, TListDto>
        where TModel : class, IEntityModel, new()
    {
        /// <summary>
        /// The repository
        /// </summary>
        /// <autogeneratedoc />
        protected readonly IRepository Repository;

        /// <inheritdoc cref="IModelConverter{TModel, TDto}" />
        protected readonly IModelConverter<TModel, TDto> ModelConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrudModelService{TModel, TDto, TQuery}"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="modelConverter">The model converter.</param>
        /// <autogeneratedoc />
        protected CrudModelService(IRepository repository, IModelConverter<TModel, TDto> modelConverter)
        {
            Repository = repository;
            ModelConverter = modelConverter;
        }

        /// <inheritdoc />
        public abstract Task<List<TListDto>> List(Guid? id, CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public async Task<TDto> Get(Guid id, CancellationToken cancellationToken = default)
            => await Repository
                .FirstOrDefault(
                    select: (TModel x) => ModelConverter.ToDto(x),
                    where: x => x.Id == id,
                    cancellationToken: cancellationToken
                );

        /// <inheritdoc />
        public async Task<TDto> Create(TDto dto)
        {
            var result = await Repository.Add(ModelConverter.FromDto(dto));
            await Repository.SaveChanges();
            return ModelConverter.ToDto(result);
        }

        /// <inheritdoc />
        public async Task<TDto> Update(TDto dto)
        {
            var result = Repository.Update(ModelConverter.FromDto(dto));
            await Repository.SaveChanges();
            return ModelConverter.ToDto(result);
        }

        /// <inheritdoc />
        public async Task<long> Delete(Guid id)
        {
            await Repository.Remove<TModel>(x => x.Id == id);
            return await Repository.SaveChanges();
        }
    }
}