using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.Commends.Commands_ItemGroup;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Products.Handlers.Handlers_ItemGroup
{
    public class DeleteItemGroupCommandHandler : IRequestHandler<DeleteItemGroupCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<ItemGroup> _repository;
        private readonly IGeneralRepository<ItemGroupItem> _itemRepository;

        public DeleteItemGroupCommandHandler(
            IGeneralRepository<ItemGroup> repository,
            IGeneralRepository<ItemGroupItem> itemRepository)
        {
            _repository = repository;
            _itemRepository = itemRepository;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeleteItemGroupCommand request, CancellationToken cancellationToken)
        {
            var itemGroup = await _repository.GetByID(request.Id);

            if (itemGroup == null)
            {
                throw new NotFoundException("Item group not found", FinanceErrorCode.NotFound);
            }

            // Check if group has items
            var hasItems = await _itemRepository.AnyAsync(x => x.GroupId == request.Id);
            if (hasItems)
            {
                throw new BusinessLogicException(
                    "Cannot delete item group with items. Please remove all items first.",
                    "Inventory",
                    FinanceErrorCode.BusinessLogicError);
            }

            await _repository.Delete(request.Id);

            return ResponseViewModel<bool>.Success(true, "Item group deleted successfully");
        }
    }
}
