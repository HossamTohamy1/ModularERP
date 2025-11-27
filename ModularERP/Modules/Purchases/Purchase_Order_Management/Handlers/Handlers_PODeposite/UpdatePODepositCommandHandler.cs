using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Purchases_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Payment.Models;
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
        private readonly IGeneralRepository<PaymentMethod> _paymentMethodRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdatePODepositCommandHandler> _logger;

        public UpdatePODepositCommandHandler(
            IGeneralRepository<PODeposit> depositRepository,
            IGeneralRepository<PurchaseOrder> poRepository,
            IGeneralRepository<PaymentMethod> paymentMethodRepository,
            IMapper mapper,
            ILogger<UpdatePODepositCommandHandler> logger)
        {
            _depositRepository = depositRepository;
            _poRepository = poRepository;
            _paymentMethodRepository = paymentMethodRepository;
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

            // ✅ 1. Validate PaymentMethod exists and is active
            var paymentMethod = await _paymentMethodRepository
                .Get(pm => pm.Id == request.PaymentMethodId && !pm.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (paymentMethod == null)
            {
                throw new NotFoundException(
                    $"Payment method with ID {request.PaymentMethodId} not found",
                    FinanceErrorCode.NotFound);
            }

            if (!paymentMethod.IsActive)
            {
                throw new BusinessLogicException(
                    $"Payment method '{paymentMethod.Name}' is currently inactive",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }

            // ✅ 2. Validate ReferenceNumber if required
            if (paymentMethod.RequiresReference && string.IsNullOrWhiteSpace(request.ReferenceNumber))
            {
                throw new BusinessLogicException(
                    $"Reference number is required for payment method '{paymentMethod.Name}'",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }

            // 3. Get existing deposit
            var deposit = await _depositRepository
                .Get(d => d.Id == request.Id && d.PurchaseOrderId == request.PurchaseOrderId && !d.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (deposit == null)
            {
                throw new NotFoundException(
                    $"Deposit with ID {request.Id} not found for the specified Purchase Order",
                    FinanceErrorCode.NotFound);
            }

            // 4. Get PO data with Select
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

            // 5. Check PO status
            if (poData.DocumentStatus == DocumentStatus.Closed || poData.DocumentStatus == DocumentStatus.Cancelled)
            {
                throw new BusinessLogicException(
                    $"Cannot update deposit for {poData.DocumentStatus} PO",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }

            // 6. Validate PO Total
            if (poData.POTotal <= 0)
            {
                throw new BusinessLogicException(
                    "Cannot update deposit for PO with no line items",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }

            // 7. Calculate new deposit amount
            decimal newDepositAmount;
            decimal? newDepositPercentage = null;

            if (request.Percentage.HasValue && request.Percentage > 0)
            {
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

            // 8. Validate total deposits don't exceed PO total
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

            // 9. Validate payment date if already paid
            if (request.AlreadyPaid && !request.PaymentDate.HasValue)
            {
                throw new BusinessLogicException(
                    "Payment date is required when AlreadyPaid is true",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }

            // 10. Store old values for logging
            var oldAmount = deposit.Amount;
            var oldAlreadyPaid = deposit.AlreadyPaid;
            var oldPaymentMethodId = deposit.PaymentMethodId;

            // 11. Update deposit
            deposit.Amount = Math.Round(newDepositAmount, 4);
            deposit.Percentage = newDepositPercentage;
            deposit.PaymentMethodId = request.PaymentMethodId; // ✅ تحديث PaymentMethodId
            deposit.ReferenceNumber = request.ReferenceNumber;
            deposit.AlreadyPaid = request.AlreadyPaid;
            deposit.PaymentDate = request.PaymentDate;
            deposit.Notes = request.Notes;
            deposit.UpdatedAt = DateTime.UtcNow;

            await _depositRepository.Update(deposit);

            // 12. Update PO Payment Status
            string newPaymentStatus = poData.PaymentStatus.ToString();

            var totalPaidDeposits = await _depositRepository
                .Get(d => d.PurchaseOrderId == request.PurchaseOrderId
                    && !d.IsDeleted
                    && d.AlreadyPaid)
                .SumAsync(d => d.Amount, cancellationToken);

            if (totalPaidDeposits >= poData.POTotal)
            {
                newPaymentStatus = "PaidInFull";
            }
            else if (totalPaidDeposits > 0)
            {
                newPaymentStatus = "PartiallyPaid";
            }
            else
            {
                newPaymentStatus = "Unpaid";
            }

            // Update PO if payment status changed
            if (newPaymentStatus != poData.PaymentStatus.ToString())
            {
                var po = await _poRepository
                    .Get(p => p.Id == request.PurchaseOrderId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (po != null)
                {
                    po.PaymentStatus = Enum.Parse<PaymentStatus>(newPaymentStatus);
                    po.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _depositRepository.SaveChanges();

            _logger.LogInformation(
                "Deposit {DepositId} updated: Old Amount={OldAmount}, New Amount={NewAmount}, " +
                "Old AlreadyPaid={OldPaid}, New AlreadyPaid={NewPaid}, " +
                "Old PaymentMethod={OldPM}, New PaymentMethod={NewPM}, " +
                "PO Total={Total}, New Payment Status={Status}",
                deposit.Id, oldAmount, deposit.Amount,
                oldAlreadyPaid, deposit.AlreadyPaid,
                oldPaymentMethodId, paymentMethod.Name,
                poData.POTotal, newPaymentStatus);

            // ✅ 13. Build response with PaymentMethod details
            var response = new PODepositResponseDto
            {
                Id = deposit.Id,
                PurchaseOrderId = deposit.PurchaseOrderId,
                Amount = deposit.Amount,
                Percentage = deposit.Percentage,
                PaymentMethodId = deposit.PaymentMethodId,
                PaymentMethodName = paymentMethod.Name,
                PaymentMethodCode = paymentMethod.Code,
                ReferenceNumber = deposit.ReferenceNumber,
                AlreadyPaid = deposit.AlreadyPaid,
                PaymentDate = deposit.PaymentDate,
                Notes = deposit.Notes,
                POTotal = poData.POTotal,
                TotalDeposits = newTotalDeposits,
                RemainingBalance = poData.POTotal - newTotalDeposits,
                PaymentStatus = newPaymentStatus,
                CreatedAt = deposit.CreatedAt,
                UpdatedAt = deposit.UpdatedAt
            };

            return ResponseViewModel<PODepositResponseDto>.Success(
                response,
                $"Deposit updated successfully using {paymentMethod.Name}");
        }
    }
}