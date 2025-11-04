using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRNItem;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Handlers.Handlers_GRNItem
{
    public class UpdateGRNItemHandler : IRequestHandler<UpdateGRNItemCommand, GRNLineItemResponseDto>
    {
        private readonly IGeneralRepository<GRNLineItem> _grnLineItemRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateGRNItemHandler> _logger;

        public UpdateGRNItemHandler(
            IGeneralRepository<GRNLineItem> grnLineItemRepository,
            IMapper mapper,
            ILogger<UpdateGRNItemHandler> logger)
        {
            _grnLineItemRepository = grnLineItemRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<GRNLineItemResponseDto> Handle(UpdateGRNItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating GRN item {ItemId} in GRN {GRNId}", request.ItemId, request.GRNId);

                var lineItem = await _grnLineItemRepository.GetByIDWithTracking(request.ItemId);
                if (lineItem == null || lineItem.GRNId != request.GRNId)
                {
                    throw new NotFoundException(
                        $"GRN line item with ID {request.ItemId} not found in GRN {request.GRNId}",
                        FinanceErrorCode.NotFound);
                }

                lineItem.POLineItemId = request.POLineItemId;
                lineItem.ReceivedQuantity = request.ReceivedQuantity;
                lineItem.Notes = request.Notes;
                lineItem.UpdatedById = request.UserId;
                lineItem.UpdatedAt = DateTime.UtcNow;

                await _grnLineItemRepository.SaveChanges();

                _logger.LogInformation("GRN item {ItemId} updated successfully", request.ItemId);

                var result = await _grnLineItemRepository
                    .GetAll()
                    .Where(l => l.Id == lineItem.Id)
                    .ProjectTo<GRNLineItemResponseDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                return result!;
            }
            catch (Exception ex) when (ex is not BaseApplicationException)
            {
                _logger.LogError(ex, "Error updating GRN item {ItemId}", request.ItemId);
                throw new BusinessLogicException(
                    $"Error updating GRN item: {ex.Message}",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }
}