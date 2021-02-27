﻿using FS.TimeTracking.Shared.Interfaces.Models;
using FS.TimeTracking.Shared.Interfaces.Services;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace FS.TimeTracking.Repository.Services
{
    /// <inheritdoc />
    public class Repository<TDbContext> : IRepository where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;
        private readonly object _saveChangesLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{TDbContext}"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <autogeneratedoc />
        public Repository(TDbContext dbContext)
            => _dbContext = dbContext;

        /// <inheritdoc />
        public Task<List<TResult>> Get<TEntity, TResult>(
            Expression<Func<TEntity, TResult>> select,
            Expression<Func<TEntity, bool>> where = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string[] includes = null,
            bool distinct = false,
            int? skip = null,
            int? take = null,
            bool tracked = false,
            CancellationToken cancellationToken = default
            ) where TEntity : class, IEntityModel
            => GetInternal(x => x.Select(select), where, orderBy, null, includes, distinct, skip, take, tracked)
                .ToListAsyncLinqToDB(cancellationToken);

        /// <inheritdoc />
        public Task<List<TResult>> GetGrouped<TEntity, TGroupByKey, TResult>(
            Expression<Func<TEntity, TGroupByKey>> groupBy,
            Expression<Func<IGrouping<TGroupByKey, TEntity>, TResult>> select,
            Expression<Func<TEntity, bool>> where = null,
            Func<IQueryable<TResult>, IOrderedQueryable<TResult>> orderBy = null,
            string[] includes = null,
            bool distinct = false,
            int? skip = null,
            int? take = null,
            bool tracked = false,
            CancellationToken cancellationToken = default
            ) where TEntity : class, IEntityModel
            => GetInternal(x => x.GroupBy(groupBy).Select(select), where, null, orderBy, includes, distinct, skip, take, tracked)
                .ToListAsyncLinqToDB(cancellationToken);

        /// <inheritdoc />
        public Task<TResult> FirstOrDefault<TEntity, TResult>(
            Expression<Func<TEntity, TResult>> select,
            Expression<Func<TEntity, bool>> where = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string[] includes = null,
            int? skip = null,
            bool tracked = false,
            CancellationToken cancellationToken = default
            ) where TEntity : class, IEntityModel
            => GetInternal(x => x.Select(select), where, orderBy, null, includes, false, skip, null, tracked)
                .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        /// <inheritdoc />
        public Task<long> Count<TEntity, TResult>(
            Expression<Func<TEntity, TResult>> select,
            Expression<Func<TEntity, bool>> where = null,
            bool distinct = false,
            CancellationToken cancellationToken = default
            ) where TEntity : class, IEntityModel
            => GetInternal(x => x.Select(select), where, null, null, null, distinct, null, null, false)
                .LongCountAsyncLinqToDB(cancellationToken);

        /// <inheritdoc />
        public Task<bool> Exists<TEntity, TResult>(
            Expression<Func<TEntity, TResult>> select,
            Expression<Func<TEntity, bool>> where = null,
            CancellationToken cancellationToken = default
            ) where TEntity : class, IEntityModel
            => GetInternal(x => x.Select(select), where, null, null, null, false, null, null, false)
                .AnyAsyncLinqToDB(cancellationToken);

        /// <inheritdoc />
        public async Task<TEntity> Add<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class, IEntityModel
            => (await AddRange(new[] { entity }.ToList(), cancellationToken)).First();

        /// <inheritdoc />
        public async Task<List<TEntity>> AddRange<TEntity>(List<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class, IEntityModel
        {
            var utcNow = DateTime.UtcNow;
            foreach (var entity in entities)
            {
                entity.Created = utcNow;
                entity.Modified = utcNow;
            }

            await _dbContext.AddRangeAsync(entities, cancellationToken);
            return entities;
        }

        /// <inheritdoc />
        public TEntity Update<TEntity>(TEntity entity) where TEntity : class, IEntityModel
        {
            entity.Modified = DateTime.UtcNow;
            return _dbContext.Update(entity).Entity;
        }

        /// <inheritdoc />
        public TEntity Remove<TEntity>(TEntity entity) where TEntity : class, IEntityModel
            => _dbContext.Remove(entity).Entity;

        /// <inheritdoc />
        public List<TEntity> Remove<TEntity>(List<TEntity> entities) where TEntity : class, IEntityModel
            => entities
                .Select(entity => _dbContext.Remove(entity).Entity)
                .ToList();

        /// <inheritdoc />
        public async Task<int> Remove<TEntity>(Expression<Func<TEntity, bool>> where = null) where TEntity : class, IEntityModel
        {
            var query = _dbContext
                .Set<TEntity>()
                .AsQueryable();

            if (where != null)
                query = query.Where(where);

            return await query.DeleteAsync();
        }

        /// <inheritdoc />
        public Task<int> SaveChanges(CancellationToken cancellationToken = default)
        {
            lock (_saveChangesLock)
                return _dbContext.SaveChangesAsync(cancellationToken);
        }

        private IQueryable<TResult> GetInternal<TEntity, TResult>(
            Func<IQueryable<TEntity>, IQueryable<TResult>> select,
            Expression<Func<TEntity, bool>> where,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> entityOrderBy,
            Func<IQueryable<TResult>, IOrderedQueryable<TResult>> resultOrderBy,
            string[] includes,
            bool distinct,
            int? skip,
            int? take,
            bool tracked
            ) where TEntity : class, IEntityModel
        {
            var query = _dbContext
                .Set<TEntity>()
                .AsQueryable();

            if (!tracked)
                query = query.AsNoTracking();

            if (where != null)
                query = query.Where(where);

            if (includes != null)
                foreach (var include in includes)
                    query = query.Include(include);

            if (entityOrderBy != null)
                query = entityOrderBy(query);

            var result = select(query);

            if (resultOrderBy != null)
                result = resultOrderBy(result);

            if (distinct)
                result = result.Distinct();

            if (skip != null)
                result = result.Skip(skip.Value);

            if (take != null)
                result = result.Take(take.Value);

            return result;
        }
    }
}