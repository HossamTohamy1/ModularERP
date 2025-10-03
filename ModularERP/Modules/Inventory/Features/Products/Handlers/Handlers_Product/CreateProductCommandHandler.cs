using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.Commends.Commends_Product;
using ModularERP.Modules.Inventory.Features.Products.DTO;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_Product;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Products.Handlers.Handlers_Product
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ResponseViewModel<ProductDetailsDto>>
    {
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IGeneralRepository<ProductStats> _statsRepository;
        private readonly IGeneralRepository<Category> _categoryRepository;
        private readonly IGeneralRepository<Brand> _brandRepository;
        private readonly IGeneralRepository<Supplier> _supplierRepository;
        private readonly IGeneralRepository<ItemGroupItem> _itemGroupItemRepository;
        private readonly IGeneralRepository<ItemGroup> _itemGroupRepository;
        private readonly IJoinTableRepository<ProductTaxProfile> _productTaxProfileRepository;
        private readonly IMapper _mapper;

        public CreateProductCommandHandler(
            IGeneralRepository<Product> productRepository,
            IGeneralRepository<ProductStats> statsRepository,
            IGeneralRepository<Category> categoryRepository,
            IGeneralRepository<Brand> brandRepository,
            IGeneralRepository<Supplier> supplierRepository,
            IGeneralRepository<ItemGroupItem> itemGroupItemRepository,
            IGeneralRepository<ItemGroup> itemGroupRepository,
            IJoinTableRepository<ProductTaxProfile> productTaxProfileRepository,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _statsRepository = statsRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _supplierRepository = supplierRepository;
            _itemGroupItemRepository = itemGroupItemRepository;
            _itemGroupRepository = itemGroupRepository;
            _productTaxProfileRepository = productTaxProfileRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<ProductDetailsDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var dto = request.ProductDto;

            // Validate business rules
            await ValidateProduct(dto);

            // Map DTO to Product entity
            var product = _mapper.Map<Product>(dto);
            product.Id = Guid.NewGuid();
            product.CreatedAt = DateTime.UtcNow;

            // ✅ FIX 1: Map PhotoUrl correctly
            product.Photo = dto.PhotoUrl;

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChanges();

            // Initialize product stats
            if (dto.TrackStock)
            {
                var stats = new ProductStats
                {
                    ProductId = product.Id,
                    CompanyId = dto.CompanyId,
                    OnHandStock = dto.InitialStock ?? 0,
                    TotalSold = 0,
                    SoldLast28Days = 0,
                    SoldLast7Days = 0,
                    // ✅ FIX 3: Ensure AvgUnitCost is set correctly
                    AvgUnitCost = dto.PurchasePrice > 0 ? dto.PurchasePrice : 0,
                    LastUpdated = DateTime.UtcNow
                };

                await _statsRepository.AddAsync(stats);
                await _statsRepository.SaveChanges();
            }

            // ✅ FIX 2: Add to ItemGroup correctly
            if (dto.ItemGroupId.HasValue)
            {
                var itemGroupItem = new ItemGroupItem
                {
                    Id = Guid.NewGuid(), // Generate new ID
                    GroupId = dto.ItemGroupId.Value,
                    ProductId = product.Id,
                    SKU = dto.SKU,
                    PurchasePrice = dto.PurchasePrice,
                    SellingPrice = dto.SellingPrice,
                    Barcode = dto.Barcode
                };

                await _itemGroupItemRepository.AddAsync(itemGroupItem);
                await _itemGroupItemRepository.SaveChanges();
            }

            // Add Tax Profiles if specified
            if (dto.TaxProfileIds != null && dto.TaxProfileIds.Any())
            {
                var productTaxProfiles = dto.TaxProfileIds.Select((taxProfileId, index) => new ProductTaxProfile
                {
                    ProductId = product.Id,
                    TaxProfileId = taxProfileId,
                    IsPrimary = index == 0 // First one is primary
                }).ToList();

                await _productTaxProfileRepository.AddRangeAsync(productTaxProfiles);
                await _productTaxProfileRepository.SaveChangesAsync();
            }

            // Get created product with full details
            var createdProduct = await _productRepository.GetByID(product.Id);

            if (createdProduct == null)
                throw new NotFoundException("Product not found after creation", Common.Enum.Finance_Enum.FinanceErrorCode.NotFound);

            // Map to response DTO
            var responseDto = _mapper.Map<ProductDetailsDto>(createdProduct);

            // ✅ Map Photo to PhotoUrl in response
            responseDto.PhotoUrl = createdProduct.Photo;

            // Load Category name
            if (createdProduct.CategoryId.HasValue)
            {
                var category = await _categoryRepository.GetByID(createdProduct.CategoryId.Value);
                responseDto.CategoryName = category?.Name ?? "N/A";
            }
            else
            {
                responseDto.CategoryName = "N/A";
            }

            // Load Brand name
            if (createdProduct.BrandId.HasValue)
            {
                var brand = await _brandRepository.GetByID(createdProduct.BrandId.Value);
                responseDto.BrandName = brand?.Name;
            }

            // Load Supplier name
            if (createdProduct.SupplierId.HasValue)
            {
                var supplier = await _supplierRepository.GetByID(createdProduct.SupplierId.Value);
                responseDto.SupplierName = supplier?.Name;
            }

            // ✅ FIX 2: Load ItemGroup name correctly
            if (dto.ItemGroupId.HasValue)
            {
                var itemGroupItem = await _itemGroupItemRepository
                    .Get(igi => igi.ProductId == createdProduct.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (itemGroupItem != null)
                {
                    var itemGroup = await _itemGroupRepository.GetByID(itemGroupItem.GroupId); // Use GroupId
                    responseDto.ItemGroupId = itemGroup?.Id;
                    responseDto.ItemGroupName = itemGroup?.Name;
                }
            }

            // Load Stats
            if (createdProduct.TrackStock)
            {
                var stats = await _statsRepository
                    .Get(s => s.ProductId == createdProduct.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (stats != null)
                {
                    responseDto.Stats = _mapper.Map<ProductStatsDto>(stats);
                    responseDto.Stats.StockStatus = GetStockStatus(stats.OnHandStock, createdProduct.LowStockThreshold);
                }
            }

            return ResponseViewModel<ProductDetailsDto>.Success(responseDto, "Product created successfully");
        }

        private async Task ValidateProduct(CreateProductDto dto)
        {
            var errors = new Dictionary<string, string[]>();

            // Check SKU uniqueness
            var skuExists = await _productRepository.AnyAsync(p =>
                p.SKU == dto.SKU &&
                p.CompanyId == dto.CompanyId);

            if (skuExists)
            {
                errors.Add("SKU", new[] { "SKU already exists for this company" });
            }

            // Check barcode uniqueness
            if (!string.IsNullOrEmpty(dto.Barcode))
            {
                var barcodeExists = await _productRepository.AnyAsync(p =>
                    p.Barcode == dto.Barcode &&
                    p.CompanyId == dto.CompanyId);

                if (barcodeExists)
                {
                    errors.Add("Barcode", new[] { "Barcode already exists for this company" });
                }
            }

            // Validate selling price vs minimum price
            if (dto.MinimumPrice.HasValue && dto.SellingPrice < dto.MinimumPrice.Value)
            {
                errors.Add("SellingPrice", new[] { "Selling price cannot be lower than minimum price" });
            }

            if (errors.Any())
            {
                throw new ValidationException("Product validation failed", errors, "Inventory");
            }
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