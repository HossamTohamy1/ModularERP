using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListAssignment;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_PriceListAssignment;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handlers_PriceListAssignment
{
    public class GetPriceListAssignmentByIdHandler : IRequestHandler<GetPriceListAssignmentByIdQuery, ResponseViewModel<PriceListAssignmentDto>>
    {
        private readonly IGeneralRepository<PriceListAssignment> _repository;
        private readonly IMapper _mapper;

        public GetPriceListAssignmentByIdHandler(
            IGeneralRepository<PriceListAssignment> repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<PriceListAssignmentDto>> Handle(
            GetPriceListAssignmentByIdQuery request,
            CancellationToken ct)
        {
            var result = await _repository
                .GetAll()
                .Where(x => x.Id == request.Id)
                .ProjectTo<PriceListAssignmentDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(ct);

            if (result == null)
            {
                throw new NotFoundException(
                    "Price list assignment not found",
                    FinanceErrorCode.NotFound);
            }

            return ResponseViewModel<PriceListAssignmentDto>.Success(
                result,
                "Price list assignment retrieved successfully");
        }
    }
}
