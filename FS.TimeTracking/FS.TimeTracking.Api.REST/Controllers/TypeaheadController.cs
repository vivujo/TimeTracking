﻿using FS.TimeTracking.Api.REST.Routing;
using FS.TimeTracking.Shared.DTOs.TimeTracking;
using FS.TimeTracking.Shared.Interfaces.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FS.TimeTracking.Api.REST.Controllers
{
    /// <inheritdoc cref="ITypeaheadService" />    
    /// <seealso cref="ControllerBase" />
    /// <seealso cref="ITypeaheadService" />
    [V1ApiController]
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
        public Task<List<TypeaheadDto<string>>> GetCustomers(CancellationToken cancellationToken = default)
            => _typeaheadService.GetCustomers(cancellationToken);

        /// <inheritdoc />
        [HttpGet]
        public Task<List<TypeaheadDto<string>>> GetProjects(CancellationToken cancellationToken = default)
            => _typeaheadService.GetProjects(cancellationToken);

        /// <inheritdoc />
        [HttpGet]
        public Task<List<TypeaheadDto<string>>> GetOrders(CancellationToken cancellationToken = default)
            => _typeaheadService.GetOrders(cancellationToken);

        /// <inheritdoc />
        [HttpGet]
        public Task<List<TypeaheadDto<string>>> GetOrderNumbers(CancellationToken cancellationToken = default)
            => _typeaheadService.GetOrderNumbers(cancellationToken);
    }
}
