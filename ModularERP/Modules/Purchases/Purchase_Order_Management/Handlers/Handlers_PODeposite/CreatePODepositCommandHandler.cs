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

        public async Task<ResponseViewModel<PODepositResponseDto>> Handle(CreatePODepositCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating deposit for Purchase Order {PurchaseOrderId}", request.PurchaseOrderId);

            // Validate PO exists
            var poExists = await _poRepository.AnyAsync(po => po.Id == request.PurchaseOrderId && !po.IsDeleted, cancellationToken);
            if (!poExists)
            {
                throw new NotFoundException($"Purchase Order with ID {request.PurchaseOrderId} not found", FinanceErrorCode.NotFound);
            }

            var deposit = new PODeposit
            {
                Id = Guid.NewGuid(),
                PurchaseOrderId = request.PurchaseOrderId,
                Amount = request.Amount,
                Percentage = request.Percentage,
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
            await _depositRepository.SaveChanges();

            _logger.LogInformation("Deposit {DepositId} created successfully for PO {PurchaseOrderId}", deposit.Id, request.PurchaseOrderId);

            var response = _mapper.Map<PODepositResponseDto>(deposit);
            return ResponseViewModel<PODepositResponseDto>.Success(response, "Deposit created successfully");
        }
    }
}
