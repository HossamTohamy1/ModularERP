using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_ImportExport;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_ImportExport;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTaking_ImportExport;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocktakingImportExportController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<StocktakingImportExportController> _logger;

        public StocktakingImportExportController(
            IMediator mediator,
            ILogger<StocktakingImportExportController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("{id}/import")]
        public async Task<ActionResult<ResponseViewModel<ImportStocktakingResultDto>>> ImportStocktaking(
            Guid id,
            [FromForm] ImportStocktakingCommand command,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Importing stocktaking data for StocktakingId: {StocktakingId}", id);

            command.StocktakingId = id;
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(ResponseViewModel<ImportStocktakingResultDto>.Success(
                result,
                "Stocktaking data imported successfully"));
        }

        [HttpGet("{id}/export")]
        public async Task<IActionResult> ExportStocktaking(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Exporting stocktaking data for StocktakingId: {StocktakingId}", id);

            var query = new ExportStocktakingQuery { StocktakingId = id };
            var result = await _mediator.Send(query, cancellationToken);

            return File(
                result.FileContent,
                result.ContentType,
                result.FileName);
        }

        [HttpGet("{id}/export-template")]
        public async Task<IActionResult> ExportStocktakingTemplate(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Exporting stocktaking template for StocktakingId: {StocktakingId}", id);

            var query = new ExportStocktakingTemplateQuery { StocktakingId = id };
            var result = await _mediator.Send(query, cancellationToken);

            return File(
                result.FileContent,
                result.ContentType,
                result.FileName);
        }

        [HttpPost("{id}/upload-image/{lineId}")]
        public async Task<ActionResult<ResponseViewModel<UploadImageResultDto>>> UploadLineImage(
            Guid id,
            Guid lineId,
            [FromForm] UploadStocktakingLineImageCommand command,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Uploading image for StocktakingId: {StocktakingId}, LineId: {LineId}",
                id,
                lineId);

            command.StocktakingId = id;
            command.LineId = lineId;
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(ResponseViewModel<UploadImageResultDto>.Success(
                result,
                "Image uploaded successfully"));
        }
    }
}