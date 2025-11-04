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
    public class AddGRNItemHandler : IRequestHandler<AddGRNItemCommand, GRNLineItemResponseDto>
    {
        private readonly IGeneralRepository<GoodsReceiptNote> _grnRepository;
        private readonly IGeneralRepository<GRNLineItem> _grnLineItemRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AddGRNItemHandler> _logger;

        public AddGRNItemHandler(
            IGeneralRepository<GoodsReceiptNote> grnRepository,
            IGeneralRepository<GRNLineItem> grnLineItemRepository,
            IMapper mapper,
            ILogger<AddGRNItemHandler> logger)
        {
            _grnRepository = grnRepository;
            _grnLineItemRepository = grnLineItemRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<GRNLineItemResponseDto> Handle(AddGRNItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Adding item to GRN {GRNId}", request.GRNId);

                var grn = await _grnRepository.GetByID(request.GRNId);
                if (grn == null)
                {
                    throw new NotFoundException(
                        $"GRN with ID {request.GRNId} not found",
                        FinanceErrorCode.NotFound);
                }

                var lineItem = new GRNLineItem
                {
                    Id = Guid.NewGuid(),
                    GRNId = request.GRNId,
                    POLineItemId = request.POLineItemId,
                    ReceivedQuantity = request.ReceivedQuantity,
                    Notes = request.Notes,
                    CreatedById = request.UserId,
                    CreatedAt = DateTime.UtcNow
                };

                await _grnLineItemRepository.AddAsync(lineItem);
                await _grnLineItemRepository.SaveChanges();

                _logger.LogInformation("Item {ItemId} added to GRN {GRNId}", lineItem.Id, request.GRNId);

                var result = await _grnLineItemRepository
                    .GetAll()
                    .Where(l => l.Id == lineItem.Id)
                    .ProjectTo<GRNLineItemResponseDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                return result!;
            }
            catch (Exception ex) when (ex is not BaseApplicationException)
            {
                _logger.LogError(ex, "Error adding item to GRN {GRNId}", request.GRNId);
                throw new BusinessLogicException(
                    $"Error adding GRN item: {ex.Message}",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }
}