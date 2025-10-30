using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commenads_POStauts;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POStatus;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Qeuries.Qeuries_POStauts;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class POStatusController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<POStatusController> _logger;

        public POStatusController(IMediator mediator, ILogger<POStatusController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get purchase order status (Reception/Payment/Document)
        /// </summary>
        [HttpGet("{id:guid}/status")]
        [ProducesResponseType(typeof(ResponseViewModel<POStatusDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStatus(Guid id)
        {
            _logger.LogInformation("API: Getting status for PO {Id}", id);
            var response = await _mediator.Send(new GetPOStatusQuery(id));
            return Ok(response);
        }

        /// <summary>
        /// Get purchase order activity log
        /// </summary>
        [HttpGet("{id:guid}/activity-log")]
        [ProducesResponseType(typeof(ResponseViewModel<List<POActivityLogDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActivityLog(Guid id)
        {
            _logger.LogInformation("API: Getting activity log for PO {Id}", id);
            var response = await _mediator.Send(new GetPOActivityLogQuery(id));
            return Ok(response);
        }

        /// <summary>
        /// Get purchase order timeline events
        /// </summary>
        [HttpGet("{id:guid}/timeline")]
        [ProducesResponseType(typeof(ResponseViewModel<POTimelineDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTimeline(Guid id)
        {
            _logger.LogInformation("API: Getting timeline for PO {Id}", id);
            var response = await _mediator.Send(new GetPOTimelineQuery(id));
            return Ok(response);
        }

        /// <summary>
        /// Update purchase order notes and terms
        /// </summary>
        [HttpPatch("{id:guid}/notes")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateNotes(Guid id, [FromBody] UpdatePONotesDto dto)
        {
            _logger.LogInformation("API: Updating notes for PO {Id}", id);
            var command = new UpdatePONotesCommand(id, dto.Notes, dto.Terms);
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        /// <summary>
        /// Get purchase order print view
        /// </summary>
        [HttpGet("{id:guid}/print")]
        [ProducesResponseType(typeof(ResponseViewModel<POPrintDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPrint(Guid id)
        {
            _logger.LogInformation("API: Getting print view for PO {Id}", id);
            var response = await _mediator.Send(new GetPOPrintQuery(id));
            return Ok(response);
        }

        /// <summary>
        /// Generate purchase order PDF
        /// </summary>
        [HttpGet("{id:guid}/pdf")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPdf(Guid id)
        {
            _logger.LogInformation("API: Generating PDF for PO {Id}", id);

            // First get the print data
            var printResponse = await _mediator.Send(new GetPOPrintQuery(id));

            if (!printResponse.IsSuccess)
            {
                return NotFound(ResponseViewModel<bool>.Error(
                    printResponse.Message,
                    printResponse.FinanceErrorCode ?? Common.Enum.Finance_Enum.FinanceErrorCode.NotFound));
            }

            // TODO: Implement PDF generation using a library like QuestPDF or iTextSharp
            // For now, return a placeholder response
            var pdfBytes = GeneratePlaceholderPdf(printResponse.Data);

            return File(pdfBytes, "application/pdf", $"PO_{printResponse.Data.PONumber}.pdf");
        }

        private byte[] GeneratePlaceholderPdf(POPrintDto data)
        {
            // Placeholder: In production, use a proper PDF library
            var content = $"Purchase Order: {data.PONumber}\n" +
                         $"Date: {data.PODate:yyyy-MM-dd}\n" +
                         $"Supplier: {data.SupplierName}\n" +
                         $"Total: {data.TotalAmount:N2} {data.CurrencyCode}";

            return System.Text.Encoding.UTF8.GetBytes(content);
        }
    }
}