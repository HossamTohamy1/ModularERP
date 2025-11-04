using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Handlers.Handlers_GRN
{
    public class DeleteGRNHandler : IRequestHandler<DeleteGRNCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<GoodsReceiptNote> _grnRepository;
        private readonly IGeneralRepository<GRNLineItem> _lineItemRepository;
        private readonly ILogger<DeleteGRNHandler> _logger;

        public DeleteGRNHandler(
            IGeneralRepository<GoodsReceiptNote> grnRepository,
            IGeneralRepository<GRNLineItem> lineItemRepository,
            ILogger<DeleteGRNHandler> logger)
        {
            _grnRepository = grnRepository;
            _lineItemRepository = lineItemRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeleteGRNCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting GRN with ID: {GRNId}", request.Id);

                // Check if GRN exists
                var grn = await _grnRepository.GetByID(request.Id);
                if (grn == null)
                {
                    throw new NotFoundException(
                        $"GRN with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                // Get all line items
                var lineItems = await _lineItemRepository.GetAll()
                    .Where(x => x.GRNId == request.Id)
                    .ToListAsync(cancellationToken);

                // Delete all line items first (soft delete)
                foreach (var lineItem in lineItems)
                {
                    await _lineItemRepository.Delete(lineItem.Id);
                }

                // Delete GRN (soft delete)
                await _grnRepository.Delete(request.Id);

                _logger.LogInformation("GRN deleted successfully: {GRNId}", request.Id);

                return ResponseViewModel<bool>.Success(
                    true,
                    "GRN deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting GRN with ID: {GRNId}", request.Id);
                throw;
            }
        }
    }
}

