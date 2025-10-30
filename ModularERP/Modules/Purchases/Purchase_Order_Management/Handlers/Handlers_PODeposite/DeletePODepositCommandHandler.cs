using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PODeposite;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_PODeposite
{
    public class DeletePODepositCommandHandler : IRequestHandler<DeletePODepositCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<PODeposit> _depositRepository;
        private readonly ILogger<DeletePODepositCommandHandler> _logger;

        public DeletePODepositCommandHandler(
            IGeneralRepository<PODeposit> depositRepository,
            ILogger<DeletePODepositCommandHandler> logger)
        {
            _depositRepository = depositRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeletePODepositCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting deposit {DepositId} for Purchase Order {PurchaseOrderId}", request.Id, request.PurchaseOrderId);

            var deposit = await _depositRepository.GetByID(request.Id);
            if (deposit == null || deposit.PurchaseOrderId != request.PurchaseOrderId)
            {
                throw new NotFoundException($"Deposit with ID {request.Id} not found for the specified Purchase Order", FinanceErrorCode.NotFound);
            }

            await _depositRepository.Delete(request.Id);

            _logger.LogInformation("Deposit {DepositId} deleted successfully", request.Id);

            return ResponseViewModel<bool>.Success(true, "Deposit deleted successfully");
        }
    }
}
