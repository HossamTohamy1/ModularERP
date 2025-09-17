using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.BankAccounts.Commands;
using ModularERP.Modules.Finance.Features.BankAccounts.DTO;
using ModularERP.Modules.Finance.Features.BankAccounts.Queries;
using ModularERP.Modules.Finance.Features.WalletPermissions.Controllers;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankAccountController : WalletPermissionsControllerBase
    {
        protected override string WalletType => "BankAccount";

        public BankAccountController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet("test/{id}")]
        public async Task<IActionResult> Test(Guid id, [FromServices] FinanceDbContext context)
        {
            try
            {
                var acc = await context.BankAccounts.FirstOrDefaultAsync(x => x.Id == id);
                if (acc == null)
                {
                    return NotFound($"Bank account with ID {id} not found");
                }
                return Ok(acc);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving bank account: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all bank accounts with pagination and filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<IEnumerable<BankAccountListDto>>>> GetAllBankAccounts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false,
            [FromQuery] Guid? companyId = null)
        {
            try
            {
                // Validate pagination parameters
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;
                if (pageSize > 100) pageSize = 100; // Limit max page size

                var query = new GetAllBankAccountsQuery
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    SearchTerm = search,
                    SortBy = sortBy,
                    SortDescending = sortDescending,
                    CompanyId = companyId
                };

                var result = await _mediator.Send(query);
                
                if (result.IsSuccess)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                var errorResponse = ResponseViewModel<IEnumerable<BankAccountListDto>>.Error(
                    $"An unexpected error occurred: {ex.Message}",
                    ModularERP.Common.Enum.Finance_Enum.FinanceErrorCode.InternalServerError);
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Get bank account by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ResponseViewModel<BankAccountDto>>> GetBankAccountById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    var errorResponse = ResponseViewModel<BankAccountDto>.Error(
                        "Invalid bank account ID",
                        ModularERP.Common.Enum.Finance_Enum.FinanceErrorCode.InvalidData);
                    return BadRequest(errorResponse);
                }

                var query = new GetBankAccountByIdQuery(id);
                var result = await _mediator.Send(query);

                if (result.IsSuccess)
                    return Ok(result);
                else if (result.FinanceErrorCode == ModularERP.Common.Enum.Finance_Enum.FinanceErrorCode.BankAccountNotFound)
                    return NotFound(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                var errorResponse = ResponseViewModel<BankAccountDto>.Error(
                    $"An unexpected error occurred: {ex.Message}",
                    ModularERP.Common.Enum.Finance_Enum.FinanceErrorCode.InternalServerError);
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Get bank accounts by company
        /// </summary>
        [HttpGet("company/{companyId:guid}")]
        public async Task<ActionResult<ResponseViewModel<IEnumerable<BankAccountListDto>>>> GetBankAccountsByCompany(
            Guid companyId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            try
            {
                if (companyId == Guid.Empty)
                {
                    var errorResponse = ResponseViewModel<IEnumerable<BankAccountListDto>>.Error(
                        "Invalid company ID",
                        ModularERP.Common.Enum.Finance_Enum.FinanceErrorCode.InvalidData);
                    return BadRequest(errorResponse);
                }

                // Validate pagination parameters
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var query = new GetBankAccountsByCompanyQuery(companyId, page, pageSize, search);
                var result = await _mediator.Send(query);

                if (result.IsSuccess)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                var errorResponse = ResponseViewModel<IEnumerable<BankAccountListDto>>.Error(
                    $"An unexpected error occurred: {ex.Message}",
                    ModularERP.Common.Enum.Finance_Enum.FinanceErrorCode.InternalServerError);
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Get bank account statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<ResponseViewModel<BankAccountStatisticsDto>>> GetBankAccountStatistics(
            [FromQuery] Guid? companyId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                // Validate date range
                if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
                {
                    var errorResponse = ResponseViewModel<BankAccountStatisticsDto>.Error(
                        "From date cannot be greater than to date",
                        ModularERP.Common.Enum.Finance_Enum.FinanceErrorCode.InvalidData);
                    return BadRequest(errorResponse);
                }

                var query = new GetBankAccountStatisticsQuery(companyId, fromDate, toDate);
                var result = await _mediator.Send(query);

                if (result.IsSuccess)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                var errorResponse = ResponseViewModel<BankAccountStatisticsDto>.Error(
                    $"An unexpected error occurred: {ex.Message}",
                    ModularERP.Common.Enum.Finance_Enum.FinanceErrorCode.InternalServerError);
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Create a new bank account
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ResponseViewModel<BankAccountCreatedDto>>> CreateBankAccount([FromBody] CreateBankAccountDto createBankAccountDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (createBankAccountDto == null)
                {
                    var errorResponse = ResponseViewModel<BankAccountCreatedDto>.Error(
                        "Bank account data is required",
                        ModularERP.Common.Enum.Finance_Enum.FinanceErrorCode.InvalidData);
                    return BadRequest(errorResponse);
                }

                var command = new CreateBankAccountCommand(createBankAccountDto);
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                    return CreatedAtAction(nameof(GetBankAccountById), new { id = result.Data?.Id }, result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                var errorResponse = ResponseViewModel<BankAccountCreatedDto>.Error(
                    $"An unexpected error occurred: {ex.Message}",
                    ModularERP.Common.Enum.Finance_Enum.FinanceErrorCode.InternalServerError);
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Update an existing bank account
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> UpdateBankAccount(Guid id, [FromBody] UpdateBankAccountDto updateBankAccountDto)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    var errorResponse = ResponseViewModel<bool>.Error(
                        "Invalid bank account ID",
                        ModularERP.Common.Enum.Finance_Enum.FinanceErrorCode.InvalidData);
                    return BadRequest(errorResponse);
                }

                if (updateBankAccountDto == null)
                {
                    var errorResponse = ResponseViewModel<bool>.Error(
                        "Bank account data is required",
                        ModularERP.Common.Enum.Finance_Enum.FinanceErrorCode.InvalidData);
                    return BadRequest(errorResponse);
                }

                if (id != updateBankAccountDto.Id)
                {
                    var errorResponse = ResponseViewModel<bool>.Error(
                        "URL ID does not match the bank account ID in the request body",
                        ModularERP.Common.Enum.Finance_Enum.FinanceErrorCode.InvalidData);
                    return BadRequest(errorResponse);
                }

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var command = new UpdateBankAccountCommand(updateBankAccountDto);
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                    return Ok(result);
                else if (result.FinanceErrorCode == ModularERP.Common.Enum.Finance_Enum.FinanceErrorCode.BankAccountNotFound)
                    return NotFound(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                var errorResponse = ResponseViewModel<bool>.Error(
                    $"An unexpected error occurred: {ex.Message}",
                    ModularERP.Common.Enum.Finance_Enum.FinanceErrorCode.InternalServerError);
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Delete a bank account
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> DeleteBankAccount(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    var errorResponse = ResponseViewModel<bool>.Error(
                        "Invalid bank account ID",
                        ModularERP.Common.Enum.Finance_Enum.FinanceErrorCode.InvalidData);
                    return BadRequest(errorResponse);
                }

                var command = new DeleteBankAccountCommand(id);
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                    return Ok(result);
                else if (result.FinanceErrorCode == ModularERP.Common.Enum.Finance_Enum.FinanceErrorCode.BankAccountNotFound)
                    return NotFound(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                var errorResponse = ResponseViewModel<bool>.Error(
                    $"An unexpected error occurred: {ex.Message}",
                    ModularERP.Common.Enum.Finance_Enum.FinanceErrorCode.InternalServerError);
                return StatusCode(500, errorResponse);
            }
        }
    }
}