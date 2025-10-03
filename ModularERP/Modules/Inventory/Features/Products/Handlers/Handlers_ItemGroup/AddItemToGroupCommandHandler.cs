using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.Commends.Commands_ItemGroup;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ItemGroup;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Products.Handlers.Handlers_ItemGroup
{
    public class AddItemToGroupCommandHandler : IRequestHandler<AddItemToGroupCommand, ResponseViewModel<ItemGroupItemDto>>
    {
        private readonly IGeneralRepository<ItemGroup> _groupRepository;
        private readonly IGeneralRepository<ItemGroupItem> _itemRepository;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IMapper _mapper;

        public AddItemToGroupCommandHandler(
            IGeneralRepository<ItemGroup> groupRepository,
            IGeneralRepository<ItemGroupItem> itemRepository,
            IGeneralRepository<Product> productRepository,
            IMapper mapper)
        {
            _groupRepository = groupRepository;
            _itemRepository = itemRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<ItemGroupItemDto>> Handle(AddItemToGroupCommand request, CancellationToken cancellationToken)
        {
            var group = await _groupRepository.GetByID(request.GroupId);
            if (group == null)
            {
                throw new NotFoundException("Item group not found", FinanceErrorCode.NotFound);
            }

            var product = await _productRepository.GetByID(request.ProductId);
            if (product == null)
            {
                throw new NotFoundException("Product not found", FinanceErrorCode.NotFound);
            }

            // Check if product already exists in group
            var exists = await _itemRepository.AnyAsync(x => x.GroupId == request.GroupId && x.ProductId == request.ProductId);
            if (exists)
            {
                throw new BusinessLogicException(
                    "Product already exists in this group",
                    "Inventory",
                    FinanceErrorCode.BusinessLogicError);
            }

            var item = _mapper.Map<ItemGroupItem>(request);
            item.Id = Guid.NewGuid();

            await _itemRepository.AddAsync(item);
            await _itemRepository.SaveChanges();

            var result = _mapper.Map<ItemGroupItemDto>(item);
            result.ProductName = product.Name;

            return ResponseViewModel<ItemGroupItemDto>.Success(result, "Item added to group successfully");
        }
    }

}
