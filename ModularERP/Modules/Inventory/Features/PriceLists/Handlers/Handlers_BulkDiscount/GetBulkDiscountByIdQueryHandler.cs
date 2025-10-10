using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_BulkDiscount;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_BulkDiscount;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handlers_BulkDiscount
{
    public class GetBulkDiscountByIdQueryHandler : IRequestHandler<GetBulkDiscountByIdQuery, ResponseViewModel<BulkDiscountDto>>
    {
        private readonly IGeneralRepository<BulkDiscount> _repository;
        private readonly IMapper _mapper;

        public GetBulkDiscountByIdQueryHandler(
            IGeneralRepository<BulkDiscount> repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<BulkDiscountDto>> Handle(GetBulkDiscountByIdQuery request, CancellationToken cancellationToken)
        {
            var discount = await _repository
                .GetAll()
                .Where(bd => bd.Id == request.Id && bd.PriceListId == request.PriceListId && !bd.IsDeleted)
                .ProjectTo<BulkDiscountDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (discount == null)
            {
                throw new Common.Exceptions.NotFoundException(
                    $"Bulk discount with ID {request.Id} not found in price list {request.PriceListId}.",
                    Common.Enum.Finance_Enum.FinanceErrorCode.NotFound);
            }

            return ResponseViewModel<BulkDiscountDto>.Success(discount, "Bulk discount retrieved successfully");
        }
    }
}