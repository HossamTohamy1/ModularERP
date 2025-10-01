using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.DTO;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Modules.Inventory.Features.Products.Qeuries;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Products.Handlers
{
    public class GetProductsListQueryHandler : IRequestHandler<GetProductsListQuery, ResponseViewModel<PaginatedProductListDto>>
    {
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IGeneralRepository<ProductStats> _statsRepository;
        private readonly IMapper _mapper;

        public GetProductsListQueryHandler(
            IGeneralRepository<Product> productRepository,
            IGeneralRepository<ProductStats> statsRepository,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _statsRepository = statsRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<PaginatedProductListDto>> Handle(GetProductsListQuery request, CancellationToken cancellationToken)
        {
            var req = request.Request;

            // Build query
            var query = _productRepository
                .GetByCompanyId(req.CompanyId)
                .AsNoTracking();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(req.SearchTerm))
            {
                var searchTerm = req.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.SKU.ToLower().Contains(searchTerm) ||
                    (p.Barcode != null && p.Barcode.ToLower().Contains(searchTerm)));
            }

            if (req.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == req.CategoryId.Value);
            }

            if (req.BrandId.HasValue)
            {
                query = query.Where(p => p.BrandId == req.BrandId.Value);
            }

            if (!string.IsNullOrWhiteSpace(req.Status))
            {
                if (Enum.TryParse<ProductStatus>(req.Status, true, out var statusEnum))
                {
                    query = query.Where(p => p.Status == statusEnum);
                }
            }

            if (req.TrackStock.HasValue)
            {
                query = query.Where(p => p.TrackStock == req.TrackStock.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = ApplySorting(query, req.SortBy, req.SortOrder);

            // Apply pagination
            var products = await query
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync(cancellationToken);

            // Map to DTOs
            var productDtos = _mapper.Map<List<ProductListItemDto>>(products);

            // Get stock data for tracked products
            if (productDtos.Any(p => p.TrackStock))
            {
                var productIds = productDtos.Where(p => p.TrackStock).Select(p => p.Id).ToList();
                var stockData = await _statsRepository
                    .Get(s => productIds.Contains(s.ProductId))
                    .Select(s => new { s.ProductId, s.OnHandStock })
                    .ToDictionaryAsync(s => s.ProductId, s => s.OnHandStock, cancellationToken);

                foreach (var product in productDtos.Where(p => p.TrackStock))
                {
                    if (stockData.TryGetValue(product.Id, out var stock))
                    {
                        product.OnHandStock = stock;
                    }
                }
            }

            var result = new PaginatedProductListDto
            {
                Items = productDtos,
                TotalCount = totalCount,
                PageNumber = req.PageNumber,
                PageSize = req.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)req.PageSize),
                HasPreviousPage = req.PageNumber > 1,
                HasNextPage = req.PageNumber * req.PageSize < totalCount
            };

            return ResponseViewModel<PaginatedProductListDto>.Success(result, "Products retrieved successfully");
        }

        private IQueryable<Product> ApplySorting(IQueryable<Product> query, string sortBy, string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "name" => isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "sku" => isDescending ? query.OrderByDescending(p => p.SKU) : query.OrderBy(p => p.SKU),
                "price" => isDescending ? query.OrderByDescending(p => p.SellingPrice) : query.OrderBy(p => p.SellingPrice),
                "createdat" => isDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };
        }
    }
}
