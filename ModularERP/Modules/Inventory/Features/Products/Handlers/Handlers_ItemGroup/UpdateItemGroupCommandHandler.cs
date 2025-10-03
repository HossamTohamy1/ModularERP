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
    public class UpdateItemGroupCommandHandler : IRequestHandler<UpdateItemGroupCommand, ResponseViewModel<ItemGroupDto>>
    {
        private readonly IGeneralRepository<ItemGroup> _repository;
        private readonly IMapper _mapper;

        public UpdateItemGroupCommandHandler(IGeneralRepository<ItemGroup> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<ItemGroupDto>> Handle(UpdateItemGroupCommand request, CancellationToken cancellationToken)
        {
            var itemGroup = await _repository.GetByID(request.Id);

            if (itemGroup == null)
            {
                throw new NotFoundException("Item group not found", FinanceErrorCode.NotFound);
            }

            _mapper.Map(request, itemGroup);

            await _repository.Update(itemGroup);

            var result = _mapper.Map<ItemGroupDto>(itemGroup);

            return ResponseViewModel<ItemGroupDto>.Success(result, "Item group updated successfully");
        }

    }
}
