using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdatePODepositCommandHandler> _logger;

        public UpdatePODepositCommandHandler(
            IGeneralRepository<PODeposit> depositRepository,
            IGeneralRepository<PurchaseOrder> poRepository,
            IMapper mapper,
            ILogger<UpdatePODepositCommandHandler> logger)
        {
            _depositRepository = depositRepository;
            _poRepository = poRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PODepositResponseDto>> Handle(
            UpdatePODepositCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Updating deposit {DepositId} for Purchase Order {PurchaseOrderId}",
                request.Id, request.PurchaseOrderId);

            // 1. Get existing deposit
            var deposit = await _depositRepository
                .Get(d => d.Id == request.Id && d.PurchaseOrderId == request.PurchaseOrderId && !d.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (deposit == null)
            {
                throw new NotFoundException(
                    $"Deposit with ID {request.Id} not found for the specified Purchase Order",
                    FinanceErrorCode.NotFound);
            }

            // 2. Get PO data with Select
            var poData = await _poRepository
                .Get(p => p.Id == request.PurchaseOrderId && !p.IsDeleted)
                .Select(p => new
                {
                    p.Id,
                    p.DocumentStatus,
                    p.PaymentStatus,
                    POTotal = p.LineItems
                        .Where(li => !li.IsDeleted)
                        .Sum(li => li.LineTotal),
                    ExistingDeposits = p.Deposits
                        .Where(d => !d.IsDeleted && d.Id != request.Id) 
                        .Sum(d => d.Amount)
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (poData == null)
                throw new NotFoundException("Purchase Order not found", FinanceErrorCode.NotFound);

            // 3. Check PO status
            if (poData.DocumentStatus == "Closed" || poData.DocumentStatus == "Cancelled")
            {
                throw new BusinessLogicException(
                    $"Cannot update deposit for {poData.DocumentStatus} PO",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }

            // 4. Validate PO Total
            if (poData.POTotal <= 0)
            {
                throw new BusinessLogicException(
                    "Cannot update deposit for PO with no line items",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }

            // 5. Calculate new deposit amount
            decimal newDepositAmount;
            decimal? newDepositPercentage = null;

            if (request.Percentage.HasValue && request.Percentage > 0)
            {
                // Validate percentage
                if (request.Percentage > 100)
                {
                    throw new BusinessLogicException(
                        "Deposit percentage cannot exceed 100%",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError);
                }

                newDepositAmount = poData.POTotal * (request.Percentage.Value / 100);
                newDepositPercentage = request.Percentage;
            }
            else if (request.Amount > 0)
            {
                newDepositAmount = request.Amount;
                newDepositPercentage = Math.Round((newDepositAmount / poData.POTotal) * 100, 2);
            }
            else
            {
                throw new BusinessLogicException(
                    "Must provide either Amount or Percentage",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }

            // 6. Validate total deposits don't exceed PO total
            var newTotalDeposits = poData.ExistingDeposits + newDepositAmount;

            if (newTotalDeposits > poData.POTotal)
            {
                throw new BusinessLogicException(
                    $"Total deposits ({newTotalDeposits:N2}) cannot exceed " +
                    $"PO total ({poData.POTotal:N2}). " +
                    $"Other deposits: {poData.ExistingDeposits:N2}",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }

            // 7. Validate payment date if already paid
            if (request.AlreadyPaid && !request.PaymentDate.HasValue)
            {
                throw new BusinessLogicException(
                    "Payment date is required when AlreadyPaid is true",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }

            // 8. Store old values for logging
            var oldAmount = deposit.Amount;
            var oldAlreadyPaid = deposit.AlreadyPaid;

            // 9. Update deposit
            deposit.Amount = Math.Round(newDepositAmount, 4);
            deposit.Percentage = newDepositPercentage;
            deposit.PaymentMethod = request.PaymentMethod;
            deposit.ReferenceNumber = request.ReferenceNumber;
            deposit.AlreadyPaid = request.AlreadyPaid;
            deposit.PaymentDate = request.PaymentDate;
            deposit.Notes = request.Notes;
            deposit.UpdatedAt = DateTime.UtcNow;

            await _depositRepository.Update(deposit);

            // 10. Update PO Payment Status
            string newPaymentStatus = poData.PaymentStatus;

            // Calculate total paid deposits (including this updated one if already paid)
            var totalPaidDeposits = await _depositRepository
                .Get(d => d.PurchaseOrderId == request.PurchaseOrderId
                    && !d.IsDeleted
                    && d.AlreadyPaid)
                .SumAsync(d => d.Amount, cancellationToken);

            if (totalPaidDeposits >= poData.POTotal)
            {
                newPaymentStatus = "Paid in Full";
            }
            else if (totalPaidDeposits > 0)
            {
                newPaymentStatus = "Partially Paid";
            }
            else
            {
                newPaymentStatus = "Unpaid";
            }

            // Update PO if payment status changed
            if (newPaymentStatus != poData.PaymentStatus)
            {
                var po = await _poRepository
                    .Get(p => p.Id == request.PurchaseOrderId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (po != null)
                {
                    po.PaymentStatus = newPaymentStatus;
                    po.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _depositRepository.SaveChanges();

            _logger.LogInformation(
                "Deposit {DepositId} updated: Old Amount={OldAmount}, New Amount={NewAmount}, " +
                "Old AlreadyPaid={OldPaid}, New AlreadyPaid={NewPaid}, " +
                "PO Total={Total}, New Payment Status={Status}",
                deposit.Id, oldAmount, deposit.Amount,
                oldAlreadyPaid, deposit.AlreadyPaid,
                poData.POTotal, newPaymentStatus);

            var response = _mapper.Map<PODepositResponseDto>(deposit);
            response.POTotal = poData.POTotal;
            response.TotalDeposits = newTotalDeposits;
            response.RemainingBalance = poData.POTotal - newTotalDeposits;
            response.PaymentStatus = newPaymentStatus;

            return ResponseViewModel<PODepositResponseDto>.Success(
                response,
                "Deposit updated successfully");
        }
    }
}