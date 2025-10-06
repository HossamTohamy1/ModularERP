using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commands_UnitTemplates;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_UnitTemplates;
using ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handelrs_UnitTemplates;
using ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries.Qeuires_UnitTemplates;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitTemplatesController : ControllerBase
    {
        private readonly IValidator<CreateUnitTemplateDto> _createValidator;
        private readonly IValidator<UpdateUnitTemplateDto> _updateValidator;
        private readonly IValidator<CreateUnitConversionDto> _conversionValidator;

        public UnitTemplatesController(
            IValidator<CreateUnitTemplateDto> createValidator,
            IValidator<UpdateUnitTemplateDto> updateValidator,
            IValidator<CreateUnitConversionDto> conversionValidator)
        {
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _conversionValidator = conversionValidator;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<List<UnitTemplateListDto>>>> GetAll(
            [FromServices] GetAllUnitTemplatesHandler handler)
        {
            var query = new GetAllUnitTemplatesQuery();
            var result = await handler.Handle(query, CancellationToken.None);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseViewModel<UnitTemplateDto>>> GetById(
            Guid id,
            [FromServices] GetUnitTemplateByIdHandler handler)
        {
            var query = new GetUnitTemplateByIdQuery(id);
            var result = await handler.Handle(query, CancellationToken.None);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ResponseViewModel<UnitTemplateDto>>> Create(
            [FromBody] CreateUnitTemplateDto dto,
            [FromServices] CreateUnitTemplateHandler handler)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                throw new ModularERP.Common.Exceptions.ValidationException(
                    "Validation failed",
                    errors,
                    "Inventory");
            }

            var command = new CreateUnitTemplateCommand(dto);
            var result = await handler.Handle(command, CancellationToken.None);
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseViewModel<UnitTemplateDto>>> Update(
            Guid id,
            [FromBody] UpdateUnitTemplateDto dto,
            [FromServices] UpdateUnitTemplateHandler handler)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                throw new ModularERP.Common.Exceptions.ValidationException(
                    "Validation failed",
                    errors,
                    "Inventory");
            }

            var command = new UpdateUnitTemplateCommand(id, dto);
            var result = await handler.Handle(command, CancellationToken.None);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> Delete(
            Guid id,
            [FromServices] DeleteUnitTemplateHandler handler)
        {
            var command = new DeleteUnitTemplateCommand(id);
            var result = await handler.Handle(command, CancellationToken.None);
            return Ok(result);
        }

        [HttpPost("{id}/units")]
        public async Task<ActionResult<ResponseViewModel<UnitConversionDto>>> AddUnitConversion(
            Guid id,
            [FromBody] CreateUnitConversionDto dto,
            [FromServices] AddUnitConversionHandler handler)
        {
            var validationResult = await _conversionValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                throw new ModularERP.Common.Exceptions.ValidationException(
                    "Validation failed",
                    errors,
                    "Inventory");
            }

            var command = new AddUnitConversionCommand(id, dto);
            var result = await handler.Handle(command, CancellationToken.None);
            return Ok(result);
        }

        [HttpDelete("{id}/units/{unitId}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> DeleteUnitConversion(
            Guid id,
            Guid unitId,
            [FromServices] DeleteUnitConversionHandler handler)
        {
            var command = new DeleteUnitConversionCommand(id, unitId);
            var result = await handler.Handle(command, CancellationToken.None);
            return Ok(result);
        }
    }
}
