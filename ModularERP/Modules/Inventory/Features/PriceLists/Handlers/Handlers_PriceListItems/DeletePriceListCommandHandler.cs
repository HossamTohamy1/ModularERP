using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceListItems;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handlers_PriceListItems
{
    public class DeletePriceListCommandHandler : IRequestHandler<DeletePriceListCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<PriceList> _repository;
        private readonly IGeneralRepository<PriceListItem> _itemRepository;

        public DeletePriceListCommandHandler(
            IGeneralRepository<PriceList> repository,
            IGeneralRepository<PriceListItem> itemRepository)
        {
            _repository = repository;
            _itemRepository = itemRepository;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeletePriceListCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var priceList = await _repository.GetByID(request.Id);
                if (priceList == null)
                {
                    throw new NotFoundException(
                        $"Price list with ID {request.Id} not found",
                        FinanceErrorCode.NotFound
                    );
                }

                // Check if price list has items (transactions)
                var hasItems = await _itemRepository.AnyAsync(x => x.PriceListId == request.Id);
                if (hasItems)
                {
                    throw new BusinessLogicException(
                        "Cannot delete price list with existing items. Please deactivate it instead.",
                        "Inventory"
                    );
                }

                // Check if it's the default price list
                if (priceList.IsDefault)
                {
                    throw new BusinessLogicException(
                        "Cannot delete the default price list. Please set another price list as default first.",
                        "Inventory"
                    );
                }

                await _repository.Delete(request.Id);

                return ResponseViewModel<bool>.Success(true, "Price list deleted successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (BusinessLogicException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException(
                    $"Error deleting price list: {ex.Message}",
                    "Inventory"
                );
            }
        }
    }
}