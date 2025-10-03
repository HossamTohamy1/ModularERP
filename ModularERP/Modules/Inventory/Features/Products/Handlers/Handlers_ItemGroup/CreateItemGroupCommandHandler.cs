using AutoMapper;
using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.Commends.Commands_ItemGroup;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ItemGroup;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Products.Handlers.Handlers_ItemGroup
{
    public class CreateItemGroupCommandHandler : IRequestHandler<CreateItemGroupCommand, ResponseViewModel<ItemGroupDto>>
    {
        private readonly IGeneralRepository<ItemGroup> _repository;
        private readonly IMapper _mapper;

        public CreateItemGroupCommandHandler(IGeneralRepository<ItemGroup> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<ItemGroupDto>> Handle(CreateItemGroupCommand request, CancellationToken cancellationToken)
        {
            var itemGroup = _mapper.Map<ItemGroup>(request);
            itemGroup.Id = Guid.NewGuid();

            await _repository.AddAsync(itemGroup);
            await _repository.SaveChanges();

            var result = _mapper.Map<ItemGroupDto>(itemGroup);

            return ResponseViewModel<ItemGroupDto>.Success(result, "Item group created successfully");
        }
    }
}
