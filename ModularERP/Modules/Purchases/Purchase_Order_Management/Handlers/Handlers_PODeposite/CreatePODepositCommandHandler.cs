using AutoMapper;
using AutoMapper.QueryableExtensions;
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
    public class CreatePODepositCommandHandler : IRequestHandler<CreatePODepositCommand, ResponseViewModel<PODepositResponseDto>>
    {
        private readonly IGeneralRepository<PODeposit> _depositRepository;
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreatePODepositCommandHandler> _logger;

        public CreatePODepositCommandHandler(
            IGeneralRepository<PODeposit> depositRepository,
            IGeneralRepository<PurchaseOrder> poRepository,
            IMapper mapper,
            ILogger<CreatePODepositCommandHandler> logger)
        {
            _depositRepository = depositRepository;
            _poRepository = poRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PODepositResponseDto>> Handle(
            CreatePODepositCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Get PO data with Select for better performance
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

            // Check PO status
            if (poData.DocumentStatus == "Closed" || poData.DocumentStatus == "Cancelled")
            {
                throw new BusinessLogicException(
                    $"Cannot add deposit to {poData.DocumentStatus} PO",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }

            // 2. Validate PO Total
            if (poData.POTotal <= 0)
            {
                throw new BusinessLogicException(
                    "Cannot add deposit to PO with no line items",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }

            // 3. Calculate deposit amount
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

            // 4. Validate total deposits don't exceed PO total
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

            // 5. Validate payment date if already paid
            if (request.AlreadyPaid && !request.PaymentDate.HasValue)
            {
                throw new BusinessLogicException(
                    "Payment date is required when AlreadyPaid is true",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }

            // 6. Create deposit
            var deposit = new PODeposit
            {
                Id = Guid.NewGuid(),
                PurchaseOrderId = request.PurchaseOrderId,
                Amount = Math.Round(depositAmount, 4),
                Percentage = depositPercentage,
                PaymentMethod = request.PaymentMethod,
                ReferenceNumber = request.ReferenceNumber,
                AlreadyPaid = request.AlreadyPaid,
                PaymentDate = request.PaymentDate,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _depositRepository.AddAsync(deposit);

            // 7. Update PO Payment Status
            string newPaymentStatus = poData.PaymentStatus;

            if (deposit.AlreadyPaid)
            {
                var totalPaidDeposits = poData.ExistingDeposits + depositAmount;

                if (totalPaidDeposits >= poData.POTotal)
                {
                    newPaymentStatus = "Paid in Full";
                }
                else if (totalPaidDeposits > 0)
                {
                    newPaymentStatus = "Partially Paid";
                }

                // Update PO using repository
                var po = await _poRepository
                    .Get(p => p.Id == request.PurchaseOrderId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (po != null)
                {
                    po.PaymentStatus = newPaymentStatus;
                    po.UpdatedAt = DateTime.UtcNow;
                }

                // TODO: Create AP prepayment entry
                // await _accountsPayableService.CreatePrepayment(deposit);
            }

            await _depositRepository.SaveChanges();

            _logger.LogInformation(
                "Deposit {DepositId} created: Amount={Amount}, " +
                "Percentage={Percentage}%, PO Total={Total}, " +
                "New Payment Status={Status}",
                deposit.Id, deposit.Amount, deposit.Percentage,
                poData.POTotal, newPaymentStatus);

            var response = _mapper.Map<PODepositResponseDto>(deposit);
            response.POTotal = poData.POTotal;
            response.TotalDeposits = newTotalDeposits;
            response.RemainingBalance = poData.POTotal - newTotalDeposits;
            response.PaymentStatus = newPaymentStatus;

            return ResponseViewModel<PODepositResponseDto>.Success(
                response,
                "Deposit created successfully");
        }
    }
}