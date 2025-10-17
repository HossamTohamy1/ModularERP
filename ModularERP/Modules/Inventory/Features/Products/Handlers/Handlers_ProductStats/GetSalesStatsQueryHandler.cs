using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ProductStats;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Modules.Inventory.Features.Products.Qeuries.Qeuries_ProductStats;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Products.Handlers.Handlers_ProductStats
{
    public class GetSalesStatsQueryHandler : IRequestHandler<GetSalesStatsQuery, SalesStatsDto>
    {
        private readonly IGeneralRepository<ProductStats> _repository;
        private readonly IMapper _mapper;

        public GetSalesStatsQueryHandler(
            IGeneralRepository<ProductStats> repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<SalesStatsDto> Handle(GetSalesStatsQuery request, CancellationToken cancellationToken)
        {
            var salesDto = await _repository
                .GetByCompanyId(request.CompanyId)
                .Where(ps => ps.ProductId == request.ProductId)
                .ProjectTo<SalesStatsDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (salesDto == null)
            {
                throw new NotFoundException(
                    $"Sales statistics not found for ProductId: {request.ProductId}",
                    FinanceErrorCode.NotFound
                );
            }

            return salesDto;
        }
    }
}