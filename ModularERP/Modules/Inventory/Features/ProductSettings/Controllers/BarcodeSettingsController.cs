using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Barcode;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Barcode;
using ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries.Qeuires_Barcode;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BarcodeSettingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BarcodeSettingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get default barcode settings
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseViewModel<BarcodeSettingsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBarcodeSettings()
        {
            var query = new GetBarcodeSettingsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get barcode settings by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ResponseViewModel<BarcodeSettingsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBarcodeSettingById(Guid id)
        {
            var query = new GetBarcodeSettingByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Create new barcode settings
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseViewModel<BarcodeSettingsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateBarcodeSettings([FromBody] CreateBarcodeSettingsDto dto)
        {
            var command = new CreateBarcodeSettingsCommand
            {
                BarcodeType = dto.BarcodeType,
                EnableWeightEmbedded = dto.EnableWeightEmbedded,
                EmbeddedBarcodeFormat = dto.EmbeddedBarcodeFormat,
                WeightUnitDivider = dto.WeightUnitDivider,
                CurrencyDivider = dto.CurrencyDivider,
                Notes = dto.Notes,
                IsDefault = dto.IsDefault
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Update existing barcode settings
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ResponseViewModel<BarcodeSettingsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBarcodeSettings(Guid id, [FromBody] UpdateBarcodeSettingsDto dto)
        {
            var command = new UpdateBarcodeSettingsCommand
            {
                Id = id,
                BarcodeType = dto.BarcodeType,
                EnableWeightEmbedded = dto.EnableWeightEmbedded,
                EmbeddedBarcodeFormat = dto.EmbeddedBarcodeFormat,
                WeightUnitDivider = dto.WeightUnitDivider,
                CurrencyDivider = dto.CurrencyDivider,
                Notes = dto.Notes,
                IsDefault = dto.IsDefault
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Delete barcode settings (soft delete)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBarcodeSettings(Guid id)
        {
            var command = new DeleteBarcodeSettingsCommand { Id = id };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Parse barcode to extract product code, weight, and price
        /// </summary>
        [HttpPost("parse")]
        [ProducesResponseType(typeof(ResponseViewModel<ParsedBarcodeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ParseBarcode([FromBody] ParseBarcodeRequest request)
        {
            var command = new ParseBarcodeCommand { Barcode = request.Barcode };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Validate barcode format and check digit
        /// </summary>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(ResponseViewModel<BarcodeValidationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ValidateBarcode([FromBody] ValidateBarcodeRequest request)
        {
            var command = new ValidateBarcodeCommand
            {
                Barcode = request.Barcode,
                BarcodeType = request.BarcodeType
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }

    // Request models for controller
    public class ParseBarcodeRequest
    {
        public string Barcode { get; set; }
    }

    public class ValidateBarcodeRequest
    {
        public string Barcode { get; set; }
        public string BarcodeType { get; set; }
    }
}