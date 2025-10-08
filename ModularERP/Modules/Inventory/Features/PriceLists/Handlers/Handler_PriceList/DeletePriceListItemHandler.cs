using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceList;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handler_PriceList
{
    public class DeletePriceListItemHandler : IRequestHandler<DeletePriceListItemCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<PriceListItem> _repository;

        public DeletePriceListItemHandler(IGeneralRepository<PriceListItem> repository)
        {
            _repository = repository;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            DeletePriceListItemCommand request,
            CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByID(request.ItemId);

            if (entity == null || entity.PriceListId != request.PriceListId)
            {
                throw new NotFoundException(
                    $"Price list item with ID {request.ItemId} not found in price list {request.PriceListId}",
                    FinanceErrorCode.NotFound);
            }

            await _repository.Delete(request.ItemId);

            return ResponseViewModel<bool>.Success(true, "Price list item deleted successfully");
        }
    }
}
