using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ItemGroup;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Modules.Inventory.Features.Products.Qeuries.Queries_ItemGroup;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Products.Handlers.Handlers_ItemGroup
{
    public class GetItemGroupByIdQueryHandler : IRequestHandler<GetItemGroupByIdQuery, ResponseViewModel<ItemGroupDetailDto>>
    {
        private readonly IGeneralRepository<ItemGroup> _repository;
        private readonly IGeneralRepository<ItemGroupItem> _itemRepository;
        private readonly IMapper _mapper;

        public GetItemGroupByIdQueryHandler(
            IGeneralRepository<ItemGroup> repository,
            IGeneralRepository<ItemGroupItem> itemRepository,
            IMapper mapper)
        {
            _repository = repository;
            _itemRepository = itemRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<ItemGroupDetailDto>> Handle(GetItemGroupByIdQuery request, CancellationToken cancellationToken)
        {
            var itemGroup = await _repository.GetByID(request.Id);

            if (itemGroup == null)
            {
                throw new NotFoundException("Item group not found", FinanceErrorCode.NotFound);
            }

            // Manually load items
            var items = await _itemRepository.Get(x => x.GroupId == request.Id).ToListAsync(cancellationToken);
            itemGroup.Items = items;

            var result = _mapper.Map<ItemGroupDetailDto>(itemGroup);

            return ResponseViewModel<ItemGroupDetailDto>.Success(result, "Item group retrieved successfully");
        }
    }
}
