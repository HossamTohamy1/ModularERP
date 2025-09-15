using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Treasuries.Commands;
using ModularERP.Modules.Finance.Features.Treasuries.Models;
using ModularERP.SharedKernel.Interfaces;

public class DeleteTreasuryHandler : IRequestHandler<DeleteTreasuryCommand, ResponseViewModel<bool>>
{
    private readonly IGeneralRepository<Treasury> _treasuryRepository;

    public DeleteTreasuryHandler(IGeneralRepository<Treasury> treasuryRepository)
    {
        _treasuryRepository = treasuryRepository;
    }

    public async Task<ResponseViewModel<bool>> Handle(DeleteTreasuryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var treasury = await _treasuryRepository
                .Get(t => t.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (treasury == null)
            {
                return ResponseViewModel<bool>.Error(
                    "Treasury not found",
                    FinanceErrorCode.TreasuryNotFound);
            }

            // Check if treasury has vouchers by counting
            var vouchersCount = await _treasuryRepository
                .Get(t => t.Id == request.Id)
                .SelectMany(t => t.Vouchers)
                .CountAsync(cancellationToken);

            if (vouchersCount > 0)
            {
                return ResponseViewModel<bool>.Error(
                    "Cannot delete treasury that has associated vouchers",
                    FinanceErrorCode.TreasuryHasVouchers);
            }

            treasury.IsDeleted = true;
            treasury.UpdatedAt = DateTime.UtcNow;

            await _treasuryRepository.Update(treasury);
            return ResponseViewModel<bool>.Success(true, "Treasury deleted successfully");
        }
        catch (Exception ex)
        {
            return ResponseViewModel<bool>.Error(
                "An error occurred while deleting the treasury",
                FinanceErrorCode.InternalServerError);
        }
    }
}