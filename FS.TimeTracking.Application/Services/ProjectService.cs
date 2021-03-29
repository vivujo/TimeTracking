﻿using AutoMapper;
using FS.TimeTracking.Shared.DTOs.TimeTracking;
using FS.TimeTracking.Shared.Interfaces.Application.Services;
using FS.TimeTracking.Shared.Interfaces.Services;
using FS.TimeTracking.Shared.Models.TimeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FS.TimeTracking.Application.Services
{
    /// <inheritdoc cref="IProjectService" />
    public class ProjectService : CrudModelService<Project, ProjectDto, ProjectListDto>, IProjectService
    {
        /// <inheritdoc />
        public ProjectService(IRepository repository, IMapper mapper)
            : base(repository, mapper)
        { }

        /// <inheritdoc />
        public override async Task<List<ProjectListDto>> List(Guid? id, CancellationToken cancellationToken = default)
            => (await base.List(id, cancellationToken)).OrderBy(x => x.Title).ToList();
    }
}
