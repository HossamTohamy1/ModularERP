using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_Product;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Modules.Inventory.Features.Products.Qeuries.Qeuries_Product;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Products.Handlers.Handlers_Product
{
    public class GetProductsListQueryHandler : IRequestHandler<GetProductsListQuery, ResponseViewModel<PaginatedProductListDto>>
    {
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IGeneralRepository<ProductStats> _statsRepository;
        private readonly IGeneralRepository<Category> _categoryRepository;
        private readonly IGeneralRepository<Brand> _brandRepository;
        private readonly IMapper _mapper;

        public GetProductsListQueryHandler(
            IGeneralRepository<Product> productRepository,
            IGeneralRepository<ProductStats> statsRepository,
            IGeneralRepository<Category> categoryRepository,
            IGeneralRepository<Brand> brandRepository,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _statsRepository = statsRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<PaginatedProductListDto>> Handle(GetProductsListQuery request, CancellationToken cancellationToken)
        {
            var req = request.Request;

            var query = _productRepository.GetAll();

            query = query.Where(p => p.CompanyId == req.CompanyId);

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

            var totalCount = await query.CountAsync(cancellationToken);

            query = ApplySorting(query, req.SortBy, req.SortOrder);

            var products = await query
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync(cancellationToken);

            var productIds = products.Select(p => p.Id).ToList();
            var categoryIds = products.Where(p => p.CategoryId.HasValue).Select(p => p.CategoryId!.Value).Distinct().ToList();
            var brandIds = products.Where(p => p.BrandId.HasValue).Select(p => p.BrandId!.Value).Distinct().ToList();

            var categoriesLookup = await _categoryRepository
                .Get(c => categoryIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

            var brandsLookup = await _brandRepository
                .Get(b => brandIds.Contains(b.Id))
                .ToDictionaryAsync(b => b.Id, b => b.Name, cancellationToken);

            var stockLookup = await _statsRepository
                .Get(s => productIds.Contains(s.ProductId))
                .ToDictionaryAsync(s => s.ProductId, s => s.OnHandStock, cancellationToken);

            var productDtos = new List<ProductListItemDto>();

            foreach (var product in products)
            {
                var dto = _mapper.Map<ProductListItemDto>(product);

                // Set Category name
                if (product.CategoryId.HasValue && categoriesLookup.ContainsKey(product.CategoryId.Value))
                {
                    dto.CategoryName = categoriesLookup[product.CategoryId.Value];
                }
                else
                {
                    dto.CategoryName = "N/A";
                }

                // Set Brand name
                if (product.BrandId.HasValue && brandsLookup.ContainsKey(product.BrandId.Value))
                {
                    dto.BrandName = brandsLookup[product.BrandId.Value];
                }

                // Set Stock
                if (product.TrackStock && stockLookup.ContainsKey(product.Id))
                {
                    dto.OnHandStock = stockLookup[product.Id];
                }

                productDtos.Add(dto);
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

        private IQueryable<Product> ApplySorting(IQueryable<Product> query, string? sortBy, string? sortOrder)
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