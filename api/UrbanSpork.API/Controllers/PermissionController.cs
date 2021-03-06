﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UrbanSpork.Common.DataTransferObjects.Permission;
using UrbanSpork.CQRS.Queries;
using UrbanSpork.CQRS.WriteModel;
using UrbanSpork.DataAccess.Projections;
using UrbanSpork.ReadModel.QueryCommands;
using UrbanSpork.WriteModel.Commands;
using UrbanSpork.WriteModel.Commands.Permission;
using UrbanSpork.WriteModel.Commands.PermissionTemplates;

namespace UrbanSpork.API.Controllers
{
    [Route("api/[controller]")]
    public class PermissionController : Controller
    {
        private readonly IQueryProcessor _queryProcessor;
        private readonly ICommandDispatcher _commandDispatcher;

        public PermissionController(IQueryProcessor queryProcessor, ICommandDispatcher commandDispatcher)
        {
            _queryProcessor = queryProcessor;
            _commandDispatcher = commandDispatcher;
        }

        [HttpGet("{Id}")]
        public async Task<PermissionDTO> Get(Guid id)
        {
            var query = new GetPermissionByIdQuery(id);
            var result = await _queryProcessor.Process(query);
            return result;
        }

        [HttpGet]
        public async Task<List<PermissionDTO>> GetAllPermissions()
        {
            var query = new GetAllPermissionsQuery();
            var result = await _queryProcessor.Process(query);
            return result;
        }

        [HttpPost("create")]
        public async Task<PermissionDTO> CreatePermission([FromBody] CreateNewPermissionDTO input)
        {
            var command = new CreatePermissionCommand(input);
            var result = await _commandDispatcher.Execute(command);
            return result;
        }

        [HttpPut("update")]
        public async Task<PermissionDTO> UpdatePermission([FromBody] UpdatePermissionInfoDTO input)
        {
            var command = new UpdatePermissionInfoCommand(input);
            var result = await _commandDispatcher.Execute(command);
            return result;
        }

        [HttpPut("disable")]
        public async Task<PermissionDTO> DisablePermission([FromBody] DisablePermissionInputDTO input)
        {
            var command = new DisablePermissionCommand(input);
            var result = await _commandDispatcher.Execute(command);
            return result;
        }

        [HttpPut("enable")]
        public async Task<PermissionDTO> EnablePermission([FromBody] EnablePermissionInputDTO input)
        {
            var command = new EnablePermissionCommand(input);
            var result = await _commandDispatcher.Execute(command);
            return result;
        }

        [HttpGet("getSystemDropdown")]
        public async Task<List<SystemDropdownProjection>> GetSystemDropDownProjection()
        {
            var query = new GetSystemDropDownProjectionQuery();
            var result = await _queryProcessor.Process(query);
            return result;
        }

        [HttpGet("getPendingRequests")]
        public async Task<List<PendingRequestsProjection>> GetPendingRequests()
        {
            var query = new GetPendingRequestsProjectionQuery();
            var result = await _queryProcessor.Process(query);
            return result;
        }

        [HttpGet("getPendingRequestsById")]
        public async Task<List<PendingRequestsProjection>> GetPendingRequestsById(Guid id)
        {
            var query = new GetPendingRequestsByIdQuery
            {
                Id = id
            };
            var result = await _queryProcessor.Process(query);
            return result;
        }

        #region Permission Templates

        [HttpGet("getTemplates")]
        public async Task<List<PermissionTemplateProjection>> GetTemplates()
        {
            var query = new GetTemplatesQuery();
            var result = await _queryProcessor.Process(query);
            return result;
        }

        [HttpPost("createPermissionTemplate")]
        public async Task<PermissionTemplateDTO> CreatePermissionTemplate([FromBody] CreatePermissionTemplateInputDTO input)
        {
            var command = new CreatePermissionTemplateCommand(input);
            var result = await _commandDispatcher.Execute(command);
            return result;
        }

        [HttpPut("editPermissionTemplate")]
        public async Task<PermissionTemplateDTO> EditPermissionTemplate([FromBody] EditPermissionTemplateInputDTO input)
        {
            var command = new EditPermissionTemplateCommand(input);
            var result = await _commandDispatcher.Execute(command);
            return result;
        }

        [HttpPut("deletePermissionTemplate")]
        public async Task<string> DeletePermissionTemplate([FromBody] DeletePermissionTemplateInputDTO input)
        {
            var command = new DeletePermissionTemplateCommand(input);
            var result = await _commandDispatcher.Execute(command);
            return result;
        }
        
        //make sure permisison disabled(remove from template) and updated(name change) events change values in templates

        #endregion
    }
}