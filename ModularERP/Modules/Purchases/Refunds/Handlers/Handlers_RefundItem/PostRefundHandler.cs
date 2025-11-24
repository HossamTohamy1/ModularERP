using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.StockTransactions.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundItem;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundItem;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Handlers.Handlers_RefundItem
{
    [Obsolete("Debit Note is created automatically in CreateRefundHandler")]
    public class PostRefundHandler : IRequestHandler<PostRefundCommand, ResponseViewModel<PostRefundResponseDto>>
    {
        private readonly IGeneralRepository<PurchaseRefund> _refundRepository;
        private readonly IGeneralRepository<DebitNote> _debitNoteRepository;
        private readonly IGeneralRepository<RefundLineItem> _refundItemRepository;
        private readonly IGeneralRepository<GRNLineItem> _grnLineRepository;
        private readonly IGeneralRepository<POLineItem> _poLineRepo;
        private readonly IGeneralRepository<WarehouseStock> _stockRepo;
        private readonly IGeneralRepository<StockTransaction> _stockTransactionRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<PostRefundHandler> _logger;

        public PostRefundHandler(
            IGeneralRepository<PurchaseRefund> refundRepository,
            IGeneralRepository<DebitNote> debitNoteRepository,
            IGeneralRepository<RefundLineItem> refundItemRepository,
            IGeneralRepository<GRNLineItem> grnLineRepository,
            IGeneralRepository<POLineItem> poLineRepo,
            IGeneralRepository<WarehouseStock> stockRepo,
            IGeneralRepository<StockTransaction> stockTransactionRepo,
            IMapper mapper,
            ILogger<PostRefundHandler> logger)
        {
            _refundRepository = refundRepository;
            _debitNoteRepository = debitNoteRepository;
            _refundItemRepository = refundItemRepository;
            _grnLineRepository = grnLineRepository;
            _poLineRepo = poLineRepo;
            _stockRepo = stockRepo;
            _stockTransactionRepo = stockTransactionRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PostRefundResponseDto>> Handle(PostRefundCommand request, CancellationToken cancellationToken)
        {
            _logger.LogWarning("PostRefundHandler is DEPRECATED. Debit Notes are now created automatically with refunds.");

            throw new BusinessLogicException(
                "This endpoint is deprecated. Debit Notes are created automatically when refunds are created. Use CreateRefundHandler instead.",
                "Purchases",
                FinanceErrorCode.BusinessLogicError);
        }
    }
}