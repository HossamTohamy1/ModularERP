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
    public class GetProductStatsQueryHandler : IRequestHandler<GetProductStatsQuery, ProductStatsDto>
    {
        private readonly IGeneralRepository<ProductStats> _repository;
        private readonly IMapper _mapper;

        public GetProductStatsQueryHandler(
            IGeneralRepository<ProductStats> repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ProductStatsDto> Handle(GetProductStatsQuery request, CancellationToken cancellationToken)
        {
            var statsDto = await _repository
                .GetByCompanyId(request.CompanyId)
                .Where(ps => ps.ProductId == request.ProductId)
                .ProjectTo<ProductStatsDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (statsDto == null)
            {
                throw new NotFoundException(
                    $"Product stats not found for ProductId: {request.ProductId}",
                    FinanceErrorCode.NotFound
                );
            }

            return statsDto;
        }
    }
}