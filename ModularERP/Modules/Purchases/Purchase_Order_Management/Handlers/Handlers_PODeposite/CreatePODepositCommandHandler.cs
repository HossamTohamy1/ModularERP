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
    public class CreatePODepositCommandHandler : IRequestHandler<CreatePODepositCommand, ResponseViewModel<PODepositResponseDto>>
    {
        private readonly IGeneralRepository<PODeposit> _depositRepository;
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly IGeneralRepository<PaymentMethod> _paymentMethodRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreatePODepositCommandHandler> _logger;

        public CreatePODepositCommandHandler(
            IGeneralRepository<PODeposit> depositRepository,
            IGeneralRepository<PurchaseOrder> poRepository,
            IGeneralRepository<PaymentMethod> paymentMethodRepository,
            IMapper mapper,
            ILogger<CreatePODepositCommandHandler> logger)
        {
            _depositRepository = depositRepository;
            _poRepository = poRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PODepositResponseDto>> Handle(
            CreatePODepositCommand request,
            CancellationToken cancellationToken)
        {
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

            // ✅ 2. Validate ReferenceNumber if required by PaymentMethod
            if (paymentMethod.RequiresReference && string.IsNullOrWhiteSpace(request.ReferenceNumber))
            {
                throw new BusinessLogicException(
                    $"Reference number is required for payment method '{paymentMethod.Name}'",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }

            // 3. Get PO data with Select for better performance
            var poData = await _poRepository
                .Get(p => p.Id == request.PurchaseOrderId && !p.IsDeleted)
                .Select(p => new
                {
                    p.Id,
                    p.DocumentStatus,
                    p.PaymentStatus,
                    p.UpdatedAt,
                    POTotal = p.LineItems
                        .Where(li => !li.IsDeleted)
                        .Sum(li => li.LineTotal),
                    ExistingDeposits = p.Deposits
                        .Where(d => !d.IsDeleted)
                        .Sum(d => d.Amount)
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (poData == null)
                throw new NotFoundException("Purchase Order not found", FinanceErrorCode.NotFound);

            // 4. Check PO status
            if (poData.DocumentStatus == DocumentStatus.Closed || poData.DocumentStatus == DocumentStatus.Cancelled)
            {
                throw new BusinessLogicException(
                    $"Cannot add deposit to {poData.DocumentStatus} PO",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }

            // 5. Validate PO Total
            if (poData.POTotal <= 0)
            {
                throw new BusinessLogicException(
                    "Cannot add deposit to PO with no line items",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }

            // 6. Calculate deposit amount
            decimal depositAmount;
            decimal? depositPercentage = null;

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

                depositAmount = poData.POTotal * (request.Percentage.Value / 100);
                depositPercentage = request.Percentage;
            }
            else if (request.Amount > 0)
            {
                depositAmount = request.Amount;
                depositPercentage = Math.Round((depositAmount / poData.POTotal) * 100, 2);
            }
            else
            {
                throw new BusinessLogicException(
                    "Must provide either Amount or Percentage",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }

            // 7. Validate total deposits don't exceed PO total
            var newTotalDeposits = poData.ExistingDeposits + depositAmount;

            if (newTotalDeposits > poData.POTotal)
            {
                throw new BusinessLogicException(
                    $"Total deposits ({newTotalDeposits:N2}) cannot exceed " +
                    $"PO total ({poData.POTotal:N2}). " +
                    $"Existing deposits: {poData.ExistingDeposits:N2}",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }

            // 8. Validate payment date if already paid
            if (request.AlreadyPaid && !request.PaymentDate.HasValue)
            {
                throw new BusinessLogicException(
                    "Payment date is required when AlreadyPaid is true",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }

            // 9. Create deposit
            var deposit = new PODeposit
            {
                Id = Guid.NewGuid(),
                PurchaseOrderId = request.PurchaseOrderId,
                Amount = Math.Round(depositAmount, 4),
                Percentage = depositPercentage,
                PaymentMethodId = request.PaymentMethodId, // ✅ استخدم PaymentMethodId
                ReferenceNumber = request.ReferenceNumber,
                AlreadyPaid = request.AlreadyPaid,
                PaymentDate = request.PaymentDate,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _depositRepository.AddAsync(deposit);

            // 10. Update PO Payment Status
            string newPaymentStatus = poData.PaymentStatus.ToString();

            if (deposit.AlreadyPaid)
            {
                var totalPaidDeposits = poData.ExistingDeposits + depositAmount;

                if (totalPaidDeposits >= poData.POTotal)
                {
                    newPaymentStatus = "PaidInFull";
                }
                else if (totalPaidDeposits > 0)
                {
                    newPaymentStatus = "PartiallyPaid";
                }

                // Update PO using repository
                var po = await _poRepository
                    .Get(p => p.Id == request.PurchaseOrderId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (po != null)
                {
                    po.PaymentStatus = Enum.Parse<PaymentStatus>(newPaymentStatus);
                    po.UpdatedAt = DateTime.UtcNow;
                }

                // TODO: Create AP prepayment entry
                // await _accountsPayableService.CreatePrepayment(deposit);
            }

            await _depositRepository.SaveChanges();

            _logger.LogInformation(
                "Deposit {DepositId} created: Amount={Amount}, " +
                "Percentage={Percentage}%, PaymentMethod={PaymentMethod}, " +
                "PO Total={Total}, New Payment Status={Status}",
                deposit.Id, deposit.Amount, deposit.Percentage,
                paymentMethod.Name, poData.POTotal, newPaymentStatus);

            // ✅ 11. Build response with PaymentMethod details
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
                CreatedAt = deposit.CreatedAt
            };

            return ResponseViewModel<PODepositResponseDto>.Success(
                response,
                $"Deposit created successfully using {paymentMethod.Name}");
        }
    }
}