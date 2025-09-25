using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Taxs.Commands;
using ModularERP.Modules.Finance.Features.Taxs.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.Taxs.Handlers
{
    public class DeleteTaxCommandHandler : IRequestHandler<DeleteTaxCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<Tax> _taxRepository;

        public DeleteTaxCommandHandler(IGeneralRepository<Tax> taxRepository)
        {
            _taxRepository = taxRepository;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeleteTaxCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Business Logic: Check if tax exists and get with related data
                var tax = await _taxRepository
                    .Get(t => t.Id == request.TaxId)
                    .Select(t => new { t.Id, t.Name, VoucherTaxesCount = t.VoucherTaxes.Count() })
                    .FirstOrDefaultAsync(cancellationToken);

                if (tax == null)
                {
                    throw new NotFoundException(
                        $"Tax with ID '{request.TaxId}' not found",
                        FinanceErrorCode.NotFound);
                }

                // Business Logic: Cannot delete tax if it has linked voucher taxes
                if (tax.VoucherTaxesCount > 0)
                {
                    throw new BusinessLogicException(
                        $"Cannot delete tax '{tax.Name}' as it is linked to {tax.VoucherTaxesCount} voucher transaction(s). Consider deactivating instead.",
                        "Finance",
                        FinanceErrorCode.BusinessLogicError);
                }

                await _taxRepository.Delete(request.TaxId);
                return ResponseViewModel<bool>.Success(true, "Tax deleted successfully");
            }
            catch (Exception ex) when (!(ex is NotFoundException || ex is BusinessLogicException))
            {
                throw new BusinessLogicException(
                    $"Error deleting tax: {ex.Message}",
                    "Finance",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }
}
