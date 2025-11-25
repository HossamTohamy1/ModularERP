using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_DebitNote;
using ModularERP.Modules.Purchases.Refunds.Qeuries.Queries_DebitNote;

namespace ModularERP.Modules.Purchases.Refunds.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierDebitNotesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SupplierDebitNotesController> _logger;

        public SupplierDebitNotesController(
            IMediator mediator,
            ILogger<SupplierDebitNotesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all Debit Notes for a specific Supplier
        /// </summary>
        /// <param name="supplierId">Supplier ID</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <param name="fromDate">Filter by note date from</param>
        /// <param name="toDate">Filter by note date to</param>
        /// <returns>List of Debit Notes for the supplier</returns>
        [HttpGet("{supplierId}/debit-notes")]
        [ProducesResponseType(typeof(ResponseViewModel<List<SupplierDebitNoteDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSupplierDebitNotes(
            Guid supplierId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            _logger.LogInformation("GET /api/suppliers/{SupplierId}/debit-notes called", supplierId);

            var query = new GetDebitNotesBySupplierQuery(supplierId)
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await _mediator.Send(query);

            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }
    }
}