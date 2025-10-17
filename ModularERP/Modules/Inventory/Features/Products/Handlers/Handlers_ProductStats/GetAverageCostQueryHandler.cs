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
    public class GetAverageCostQueryHandler : IRequestHandler<GetAverageCostQuery, AverageCostDto>
    {
        private readonly IGeneralRepository<ProductStats> _repository;
        private readonly IMapper _mapper;

        public GetAverageCostQueryHandler(
            IGeneralRepository<ProductStats> repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<AverageCostDto> Handle(GetAverageCostQuery request, CancellationToken cancellationToken)
        {
            var costDto = await _repository
                .GetByCompanyId(request.CompanyId)
                .Where(ps => ps.ProductId == request.ProductId)
                .ProjectTo<AverageCostDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (costDto == null)
            {
                throw new NotFoundException(
                    $"Average cost information not found for ProductId: {request.ProductId}",
                    FinanceErrorCode.NotFound
                );
            }

            return costDto;
        }
    }
}