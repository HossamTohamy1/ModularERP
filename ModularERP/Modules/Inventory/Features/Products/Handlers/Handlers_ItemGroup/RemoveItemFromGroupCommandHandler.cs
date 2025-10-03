using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.Commends.Commands_ItemGroup;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Products.Handlers.Handlers_ItemGroup
{
    public class RemoveItemFromGroupCommandHandler : IRequestHandler<RemoveItemFromGroupCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<ItemGroupItem> _itemRepository;

        public RemoveItemFromGroupCommandHandler(IGeneralRepository<ItemGroupItem> itemRepository)
        {
            _itemRepository = itemRepository;
        }

        public async Task<ResponseViewModel<bool>> Handle(RemoveItemFromGroupCommand request, CancellationToken cancellationToken)
        {
            var item = await _itemRepository.GetByID(request.ItemId);

            if (item == null || item.GroupId != request.GroupId)
            {
                throw new NotFoundException("Item not found in this group", FinanceErrorCode.NotFound);
            }

            await _itemRepository.Delete(request.ItemId);

            return ResponseViewModel<bool>.Success(true, "Item removed from group successfully");
        }
    }
}
