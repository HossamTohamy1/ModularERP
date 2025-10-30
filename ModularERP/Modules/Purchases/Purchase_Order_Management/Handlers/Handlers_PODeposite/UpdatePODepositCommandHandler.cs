using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PODeposite;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PODeposite;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_PODeposite
{
    public class UpdatePODepositCommandHandler : IRequestHandler<UpdatePODepositCommand, ResponseViewModel<PODepositResponseDto>>
    {
        private readonly IGeneralRepository<PODeposit> _depositRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdatePODepositCommandHandler> _logger;

        public UpdatePODepositCommandHandler(
            IGeneralRepository<PODeposit> depositRepository,
            IMapper mapper,
            ILogger<UpdatePODepositCommandHandler> logger)
        {
            _depositRepository = depositRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PODepositResponseDto>> Handle(UpdatePODepositCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating deposit {DepositId} for Purchase Order {PurchaseOrderId}", request.Id, request.PurchaseOrderId);

            var deposit = await _depositRepository.GetByIDWithTracking(request.Id);
            if (deposit == null || deposit.PurchaseOrderId != request.PurchaseOrderId)
            {
                throw new NotFoundException($"Deposit with ID {request.Id} not found for the specified Purchase Order", FinanceErrorCode.NotFound);
            }

            deposit.Amount = request.Amount;
            deposit.Percentage = request.Percentage;
            deposit.PaymentMethod = request.PaymentMethod;
            deposit.ReferenceNumber = request.ReferenceNumber;
            deposit.AlreadyPaid = request.AlreadyPaid;
            deposit.PaymentDate = request.PaymentDate;
            deposit.Notes = request.Notes;
            deposit.UpdatedAt = DateTime.UtcNow;

            await _depositRepository.Update(deposit);

            _logger.LogInformation("Deposit {DepositId} updated successfully", request.Id);

            var response = _mapper.Map<PODepositResponseDto>(deposit);
            return ResponseViewModel<PODepositResponseDto>.Success(response, "Deposit updated successfully");
        }
    }
}
