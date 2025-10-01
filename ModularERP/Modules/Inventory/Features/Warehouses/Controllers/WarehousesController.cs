using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Warehouses.Commends;
using ModularERP.Modules.Inventory.Features.Warehouses.DTO;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Validators;
using Serilog;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehousesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WarehousesController(IMediator mediator)
        {
            _mediator = mediator;
        }

     
        [HttpGet]
        [ProducesResponseType(typeof(ResponseViewModel<PagedResult<WarehouseListDto>>), 200)]
        public async Task<ActionResult<ResponseViewModel<PagedResult<WarehouseListDto>>>> GetWarehouses(
            [FromQuery] Guid? companyId,
            [FromQuery] string? status,
            [FromQuery] bool? isPrimary,
            [FromQuery] string? searchTerm,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetWarehousesQuery
            {
                CompanyId = companyId,
                Status = status,
                IsPrimary = isPrimary,
                SearchTerm = searchTerm,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(ResponseViewModel<PagedResult<WarehouseListDto>>.Success(
                result,
                "Warehouses retrieved successfully"));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<WarehouseDto>), 200)]
        [ProducesResponseType(typeof(ResponseViewModel<WarehouseDto>), 404)]
        public async Task<ActionResult<ResponseViewModel<WarehouseDto>>> GetWarehouse(Guid id)
        {
            var query = new GetWarehouseByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            return Ok(ResponseViewModel<WarehouseDto>.Success(
                result,
                "Warehouse retrieved successfully"));
        }


        [HttpPost]
        [ProducesResponseType(typeof(ResponseViewModel<WarehouseDto>), 201)]
        [ProducesResponseType(typeof(ResponseViewModel<WarehouseDto>), 400)]
        public async Task<ActionResult<ResponseViewModel<WarehouseDto>>> CreateWarehouse(
            [FromBody] CreateWarehouseDto dto)
        {
            var validator = new CreateWarehouseDtoValidator();
            var validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                throw new ValidationException(
                    "Validation failed",
                    errors,
                    "Inventory");
            }

            var command = new CreateWarehouseCommand
            {
                Name = dto.Name,
                ShippingAddress = dto.ShippingAddress,
                Status = dto.Status,
                IsPrimary = dto.IsPrimary,
                CompanyId = dto.CompanyId
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(
                nameof(GetWarehouse),
                new { id = result.Id },
                ResponseViewModel<WarehouseDto>.Success(
                    result,
                    "Warehouse created successfully"));
        }


        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<WarehouseDto>), 200)]
        [ProducesResponseType(typeof(ResponseViewModel<WarehouseDto>), 400)]
        [ProducesResponseType(typeof(ResponseViewModel<WarehouseDto>), 404)]
        public async Task<ActionResult<ResponseViewModel<WarehouseDto>>> UpdateWarehouse(
            Guid id,
            [FromBody] UpdateWarehouseDto dto)
        {
            var validator = new UpdateWarehouseDtoValidator();
            var validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                throw new ValidationException(
                    "Validation failed",
                    errors,
                    "Inventory");
            }

            var command = new UpdateWarehouseCommand
            {
                Id = id,
                Name = dto.Name,
                ShippingAddress = dto.ShippingAddress,
                Status = dto.Status,
                IsPrimary = dto.IsPrimary,
                CompanyId = dto.CompanyId
            };

            var result = await _mediator.Send(command);
            return Ok(ResponseViewModel<WarehouseDto>.Success(
                result,
                "Warehouse updated successfully"));
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), 200)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), 400)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), 404)]
        public async Task<ActionResult<ResponseViewModel<bool>>> DeleteWarehouse(
            Guid id)
        {
            var command = new DeleteWarehouseCommand
            {
                Id = id,
            };

            var result = await _mediator.Send(command);
            return Ok(ResponseViewModel<bool>.Success(
                result,
                "Warehouse deleted successfully"));
        }

        [HttpPatch("{id}/status")]
        [ProducesResponseType(typeof(ResponseViewModel<WarehouseDto>), 200)]
        [ProducesResponseType(typeof(ResponseViewModel<WarehouseDto>), 400)]
        [ProducesResponseType(typeof(ResponseViewModel<WarehouseDto>), 404)]
        public async Task<ActionResult<ResponseViewModel<WarehouseDto>>> UpdateWarehouseStatus(
            Guid id,
            [FromBody] UpdateWarehouseStatusDto dto)
        {
            var validator = new UpdateWarehouseStatusDtoValidator();
            var validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                throw new ValidationException(
                    "Validation failed",
                    errors,
                    "Inventory");
            }

            var command = new UpdateWarehouseStatusCommand
            {
                Id = id,
                Status = dto.Status,
            };

            var result = await _mediator.Send(command);
            return Ok(ResponseViewModel<WarehouseDto>.Success(
                result,
                "Warehouse status updated successfully"));
        }

        [HttpPatch("{id}/set-primary")]
        [ProducesResponseType(typeof(ResponseViewModel<WarehouseDto>), 200)]
        [ProducesResponseType(typeof(ResponseViewModel<WarehouseDto>), 400)]
        [ProducesResponseType(typeof(ResponseViewModel<WarehouseDto>), 404)]
        public async Task<ActionResult<ResponseViewModel<WarehouseDto>>> SetPrimaryWarehouse(
            Guid id)
        {
            var command = new SetPrimaryWarehouseCommand
            {
                Id = id,

            };

            var result = await _mediator.Send(command);
            return Ok(ResponseViewModel<WarehouseDto>.Success(
                result,
                "Warehouse set as primary successfully"));
        }
    }
}


