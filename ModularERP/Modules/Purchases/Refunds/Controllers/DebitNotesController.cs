using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_DebitNote;
using ModularERP.Modules.Purchases.Refunds.Qeuries.Queries_DebitNote;

namespace ModularERP.Modules.Purchases.Refunds.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebitNotesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DebitNotesController> _logger;

        public DebitNotesController(
            IMediator mediator,
            ILogger<DebitNotesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all Debit Notes with pagination and filters
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <param name="searchTerm">Search in debit note number and notes</param>
        /// <param name="fromDate">Filter by note date from</param>
        /// <param name="toDate">Filter by note date to</param>
        /// <param name="supplierId">Filter by supplier</param>
        /// <returns>List of Debit Notes</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseViewModel<List<DebitNoteListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllDebitNotes(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] Guid? supplierId = null)
        {
            _logger.LogInformation("GET /api/debit-notes called - Page: {PageNumber}, Size: {PageSize}",
                pageNumber, pageSize);

            var query = new GetAllDebitNotesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                FromDate = fromDate,
                ToDate = toDate,
                SupplierId = supplierId
            };

            var result = await _mediator.Send(query);

            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }

        /// <summary>
        /// Get Debit Note by ID
        /// </summary>
        /// <param name="id">Debit Note ID</param>
        /// <returns>Debit Note details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<DebitNoteDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDebitNoteById(Guid id)
        {
            _logger.LogInformation("GET /api/debit-notes/{Id} called", id);

            var query = new GetDebitNoteByIdQuery(id);
            var result = await _mediator.Send(query);

            return result.IsSuccess
                ? Ok(result)
                : NotFound(result);
        }

        /// <summary>
        /// Get Refund linked to Debit Note
        /// </summary>
        /// <param name="id">Debit Note ID</param>
        /// <returns>Refund summary</returns>
        [HttpGet("{id}/refund")]
        [ProducesResponseType(typeof(ResponseViewModel<RefundSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDebitNoteRefund(Guid id)
        {
            _logger.LogInformation("GET /api/debit-notes/{Id}/refund called", id);

            var query = new GetDebitNoteRefundQuery(id);
            var result = await _mediator.Send(query);

            return result.IsSuccess
                ? Ok(result)
                : NotFound(result);
        }
    }
}