using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.GlAccounts.Commands;
using ModularERP.Modules.Finance.Features.GlAccounts.Models;
using ModularERP.Shared.Interfaces;
using Serilog;
using ILogger = Serilog.ILogger;

namespace ModularERP.Modules.Finance.Features.GlAccounts.Handlers
{
    public class DeleteGlAccountHandler : IRequestHandler<DeleteGlAccountCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<GlAccount> _repository;
        private readonly ILogger _logger = Log.ForContext<DeleteGlAccountHandler>();

        public DeleteGlAccountHandler(IGeneralRepository<GlAccount> repository)
        {
            _repository = repository;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeleteGlAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.Information("Starting GLAccount deletion process for ID: {Id}", request.Id);

                // Check if GLAccount exists
                var existingAccount = await _repository.GetByID(request.Id);
                if (existingAccount == null)
                {
                    _logger.Warning("GLAccount deletion failed - Account with ID {Id} not found", request.Id);
                    throw new NotFoundException(
                        $"GLAccount with ID '{request.Id}' not found",
                        FinanceErrorCode.NotFound);
                }

                // Check if account has related records (vouchers or ledger entries)
                var hasVouchers = await _repository.AnyAsync(x => x.Id == request.Id &&
                    (x.CategoryVouchers.Any() || x.JournalVouchers.Any() || x.LedgerEntries.Any()));

                if (hasVouchers)
                {
                    _logger.Warning("GLAccount deletion failed - Account with ID {Id} has related vouchers or ledger entries", request.Id);
                    throw new BusinessLogicException(
                        "Cannot delete GLAccount because it has related vouchers or ledger entries",
                        "Finance",
                        FinanceErrorCode.BusinessLogicError);
                }

                await _repository.Delete(request.Id);

                _logger.Information("GLAccount deleted successfully with ID: {Id}, Code: {Code}",
                    existingAccount.Id, existingAccount.Code);

                return ResponseViewModel<bool>.Success(true, "GLAccount deleted successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (BusinessLogicException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while deleting GLAccount with ID: {Id}", request.Id);
                throw new BusinessLogicException(
                    "An error occurred while deleting the GLAccount",
                    "Finance",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
