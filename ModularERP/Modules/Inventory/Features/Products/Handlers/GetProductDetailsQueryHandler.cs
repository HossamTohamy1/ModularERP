using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.DTO;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Modules.Inventory.Features.Products.Qeuries;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Products.Handlers
{
    public class GetProductDetailsQueryHandler : IRequestHandler<GetProductDetailsQuery, ResponseViewModel<ProductDetailsDto>>
    {
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IGeneralRepository<ProductStats> _statsRepository;
        private readonly IMapper _mapper;

        public GetProductDetailsQueryHandler(
            IGeneralRepository<Product> productRepository,
            IGeneralRepository<ProductStats> statsRepository,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _statsRepository = statsRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<ProductDetailsDto>> Handle(GetProductDetailsQuery request, CancellationToken cancellationToken)
        {
            // Get product
            var product = await _productRepository
                .Get(p => p.Id == request.ProductId && p.CompanyId == request.CompanyId)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (product == null)
            {
                throw new NotFoundException(
                    "Product not found or does not belong to this company",
                    FinanceErrorCode.NotFound);
            }

            // Map Product to DetailsDto
            var detailsDto = _mapper.Map<ProductDetailsDto>(product);

            // Get stats if product tracks stock
            if (product.TrackStock)
            {
                var stats = await _statsRepository
                    .Get(s => s.ProductId == product.Id)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken);

                if (stats != null)
                {
                    // Map stats to DTO
                    detailsDto.Stats = _mapper.Map<ProductStatsDto>(stats);
                    detailsDto.Stats.StockStatus = GetStockStatus(stats.OnHandStock, product.LowStockThreshold);
                }
            }

            return ResponseViewModel<ProductDetailsDto>.Success(detailsDto, "Product details retrieved successfully");
        }

        private string GetStockStatus(decimal onHandStock, decimal? lowStockThreshold)
        {
            if (onHandStock <= 0)
                return "OutOfStock";

            if (lowStockThreshold.HasValue && onHandStock <= lowStockThreshold.Value)
                return "LowStock";

            return "InStock";
        }
    }
}

