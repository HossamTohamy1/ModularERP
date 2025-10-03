using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.DTO;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_Product;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Modules.Inventory.Features.Products.Qeuries.Qeuries_Product;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Products.Handlers.Handlers_Product
{
    public class GetProductDetailsQueryHandler : IRequestHandler<GetProductDetailsQuery, ResponseViewModel<ProductDetailsDto>>
    {
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IGeneralRepository<ProductStats> _statsRepository;
        private readonly IGeneralRepository<Category> _categoryRepository;
        private readonly IGeneralRepository<Brand> _brandRepository;
        private readonly IGeneralRepository<Supplier> _supplierRepository;
        private readonly IGeneralRepository<ItemGroupItem> _itemGroupItemRepository;
        private readonly IGeneralRepository<ItemGroup> _itemGroupRepository;
        private readonly IMapper _mapper;

        public GetProductDetailsQueryHandler(
            IGeneralRepository<Product> productRepository,
            IGeneralRepository<ProductStats> statsRepository,
            IGeneralRepository<Category> categoryRepository,
            IGeneralRepository<Brand> brandRepository,
            IGeneralRepository<Supplier> supplierRepository,
            IGeneralRepository<ItemGroupItem> itemGroupItemRepository,
            IGeneralRepository<ItemGroup> itemGroupRepository,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _statsRepository = statsRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _supplierRepository = supplierRepository;
            _itemGroupItemRepository = itemGroupItemRepository;
            _itemGroupRepository = itemGroupRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<ProductDetailsDto>> Handle(GetProductDetailsQuery request, CancellationToken cancellationToken)
        {
            // Get product
            var product = await _productRepository.GetByID(request.ProductId);

            if (product == null || product.CompanyId != request.CompanyId)
            {
                throw new NotFoundException(
                    "Product not found or does not belong to this company",
                    FinanceErrorCode.NotFound);
            }

            // Map Product to DetailsDto
            var detailsDto = _mapper.Map<ProductDetailsDto>(product);

            // ✅ FIX 1: Map Photo to PhotoUrl
            detailsDto.PhotoUrl = product.Photo;

            // Load Category name
            if (product.CategoryId.HasValue)
            {
                var category = await _categoryRepository.GetByID(product.CategoryId.Value);
                detailsDto.CategoryName = category?.Name ?? "N/A";
            }
            else
            {
                detailsDto.CategoryName = "N/A";
            }

            // Load Brand name
            if (product.BrandId.HasValue)
            {
                var brand = await _brandRepository.GetByID(product.BrandId.Value);
                detailsDto.BrandName = brand?.Name;
            }

            // Load Supplier name
            if (product.SupplierId.HasValue)
            {
                var supplier = await _supplierRepository.GetByID(product.SupplierId.Value);
                detailsDto.SupplierName = supplier?.Name;
            }

            // ✅ FIX 2: Load ItemGroup correctly using GroupId
            var itemGroupItem = await _itemGroupItemRepository
                .Get(igi => igi.ProductId == product.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (itemGroupItem != null)
            {
                // Use GroupId instead of Id
                var itemGroup = await _itemGroupRepository.GetByID(itemGroupItem.GroupId);
                detailsDto.ItemGroupId = itemGroup?.Id;
                detailsDto.ItemGroupName = itemGroup?.Name;
            }

            // Get stats if product tracks stock
            if (product.TrackStock)
            {
                var stats = await _statsRepository
                    .Get(s => s.ProductId == product.Id)
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