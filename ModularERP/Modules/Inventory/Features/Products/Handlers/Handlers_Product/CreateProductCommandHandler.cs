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
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
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
        private readonly IGeneralRepository<Warehouse> _warehouseRepository;
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
            IGeneralRepository<Warehouse> warehouseRepository,
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
            _warehouseRepository = warehouseRepository;
            _itemGroupItemRepository = itemGroupItemRepository;
            _itemGroupRepository = itemGroupRepository;
            _productTaxProfileRepository = productTaxProfileRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<ProductDetailsDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var dto = request.ProductDto;

            await ValidateProduct(dto);

            var product = _mapper.Map<Product>(dto);
            product.Id = Guid.NewGuid();
            product.CreatedAt = DateTime.UtcNow;
            product.Photo = dto.PhotoUrl;
            product.ProfitMargin = CalculateProfitMargin(dto.PurchasePrice, dto.SellingPrice);

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChanges();

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
                    AvgUnitCost = dto.PurchasePrice > 0 ? dto.PurchasePrice : 0,
                    LastUpdated = DateTime.UtcNow
                };

                await _statsRepository.AddAsync(stats);
                await _statsRepository.SaveChanges();
            }

            if (dto.ItemGroupId.HasValue)
            {
                var itemGroupItem = new ItemGroupItem
                {
                    Id = Guid.NewGuid(),
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

            if (dto.TaxProfileIds != null && dto.TaxProfileIds.Any())
            {
                var productTaxProfiles = dto.TaxProfileIds.Select((taxProfileId, index) => new ProductTaxProfile
                {
                    ProductId = product.Id,
                    TaxProfileId = taxProfileId,
                    IsPrimary = index == 0
                }).ToList();

                await _productTaxProfileRepository.AddRangeAsync(productTaxProfiles);
                await _productTaxProfileRepository.SaveChangesAsync();
            }

            var createdProduct = await _productRepository.GetByID(product.Id);
            if (createdProduct == null)
                throw new NotFoundException("Product not found after creation", Common.Enum.Finance_Enum.FinanceErrorCode.NotFound);

            var responseDto = await MapToDetailsDto(createdProduct, cancellationToken);
            return ResponseViewModel<ProductDetailsDto>.Success(responseDto, "Product created successfully");
        }

        private async Task ValidateProduct(CreateProductDto dto)
        {
            var errors = new Dictionary<string, string[]>();

            var skuExists = await _productRepository.AnyAsync(p =>
                p.SKU == dto.SKU &&
                p.CompanyId == dto.CompanyId);

            if (skuExists)
                errors.Add("SKU", new[] { "SKU already exists for this company" });

            if (!string.IsNullOrEmpty(dto.Barcode))
            {
                var barcodeExists = await _productRepository.AnyAsync(p =>
                    p.Barcode == dto.Barcode &&
                    p.CompanyId == dto.CompanyId);

                if (barcodeExists)
                    errors.Add("Barcode", new[] { "Barcode already exists for this company" });
            }

            // Validate Warehouse
            var warehouseExists = await _warehouseRepository.AnyAsync(w =>
                w.Id == dto.WarehouseId &&
                w.CompanyId == dto.CompanyId &&
                w.Status == Common.Enum.Inventory_Enum.WarehouseStatus.Active);

            if (!warehouseExists)
                errors.Add("WarehouseId", new[] { "Warehouse not found or not active" });

            if (dto.MinimumPrice.HasValue && dto.SellingPrice < dto.MinimumPrice.Value)
                errors.Add("SellingPrice", new[] { "Selling price cannot be lower than minimum price" });

            if (errors.Any())
                throw new ValidationException("Product validation failed", errors, "Inventory");
        }

        private decimal? CalculateProfitMargin(decimal purchasePrice, decimal sellingPrice)
        {
            if (sellingPrice > 0 && purchasePrice > 0)
            {
                return (sellingPrice - purchasePrice) / purchasePrice * 100;
            }
            return null;
        }


        private async Task<ProductDetailsDto> MapToDetailsDto(Product product, CancellationToken cancellationToken)
        {
            var responseDto = _mapper.Map<ProductDetailsDto>(product);
            responseDto.PhotoUrl = product.Photo;

            // Load Warehouse
            if (product.WarehouseId != Guid.Empty)
            {
                var warehouse = await _warehouseRepository.GetByID(product.WarehouseId);
                responseDto.WarehouseName = warehouse?.Name;
            }

            // Load Category
            if (product.CategoryId.HasValue)
            {
                var category = await _categoryRepository.GetByID(product.CategoryId.Value);
                responseDto.CategoryName = category?.Name ?? "N/A";
            }
            else
            {
                responseDto.CategoryName = "N/A";
            }

            // Load Brand
            if (product.BrandId.HasValue)
            {
                var brand = await _brandRepository.GetByID(product.BrandId.Value);
                responseDto.BrandName = brand?.Name;
            }

            // Load Supplier
            if (product.SupplierId.HasValue)
            {
                var supplier = await _supplierRepository.GetByID(product.SupplierId.Value);
                responseDto.SupplierName = supplier?.Name;
            }

            // Load ItemGroup
            var itemGroupItem = await _itemGroupItemRepository
                .Get(igi => igi.ProductId == product.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (itemGroupItem != null)
            {
                var itemGroup = await _itemGroupRepository.GetByID(itemGroupItem.GroupId);
                responseDto.ItemGroupId = itemGroup?.Id;
                responseDto.ItemGroupName = itemGroup?.Name;
            }

            // Load Stats
            if (product.TrackStock)
            {
                var stats = await _statsRepository
                    .Get(s => s.ProductId == product.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (stats != null)
                {
                    responseDto.Stats = _mapper.Map<ProductStatsDto>(stats);
                    responseDto.Stats.StockStatus = GetStockStatus(stats.OnHandStock, product.LowStockThreshold);
                }
            }

            return responseDto;
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