using AutoMapper;
using MediatR;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_Product;
using ModularERP.Modules.Inventory.Features.Products.DTO;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using ModularERP.Modules.Inventory.Features.Products.Commends.Commends_Product;

namespace ModularERP.Modules.Inventory.Features.Products.Handlers.Handlers_Product
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ResponseViewModel<ProductDetailsDto>>
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

        public UpdateProductCommandHandler(
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

        public async Task<ResponseViewModel<ProductDetailsDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var dto = request.ProductDto;

            var existingProduct = await _productRepository.GetByID(dto.Id);

            if (existingProduct == null || existingProduct.CompanyId != dto.CompanyId)
            {
                throw new NotFoundException(
                    "Product not found or does not belong to this company",
                    Common.Enum.Finance_Enum.FinanceErrorCode.NotFound);
            }

            await ValidateProductUpdate(dto, existingProduct.Id);

            existingProduct.Name = dto.Name;
            existingProduct.SKU = dto.SKU;
            existingProduct.Description = dto.Description;
            existingProduct.Photo = dto.PhotoUrl;
            existingProduct.WarehouseId = dto.WarehouseId;  // ✅ NEW
            existingProduct.CategoryId = dto.CategoryId;
            existingProduct.BrandId = dto.BrandId;
            existingProduct.SupplierId = dto.SupplierId;
            existingProduct.Barcode = dto.Barcode;
            existingProduct.PurchasePrice = dto.PurchasePrice;
            existingProduct.SellingPrice = dto.SellingPrice;
            existingProduct.MinPrice = dto.MinimumPrice;
            existingProduct.Discount = dto.Discount;
            existingProduct.DiscountType = string.IsNullOrEmpty(dto.DiscountType)
                ? null
                : Enum.Parse<Common.Enum.Inventory_Enum.DiscountType>(dto.DiscountType);
            existingProduct.ProfitMargin = CalculateProfitMargin(dto.PurchasePrice, dto.SellingPrice);
            existingProduct.TrackStock = dto.TrackStock;
            existingProduct.LowStockThreshold = dto.LowStockThreshold;
            existingProduct.InternalNotes = dto.InternalNotes;
            existingProduct.Tags = dto.Tags;
            existingProduct.Status = Enum.Parse<Common.Enum.Inventory_Enum.ProductStatus>(dto.Status);
            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _productRepository.Update(existingProduct);

            var existingItemGroupItem = await _itemGroupItemRepository
                .Get(igi => igi.ProductId == existingProduct.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (dto.ItemGroupId.HasValue)
            {
                if (existingItemGroupItem != null)
                {
                    if (existingItemGroupItem.GroupId != dto.ItemGroupId.Value)
                    {
                        await _itemGroupItemRepository.Delete(existingItemGroupItem.Id);

                        var newItemGroupItem = new ItemGroupItem
                        {
                            Id = Guid.NewGuid(),
                            GroupId = dto.ItemGroupId.Value,
                            ProductId = existingProduct.Id,
                            SKU = dto.SKU,
                            PurchasePrice = dto.PurchasePrice,
                            SellingPrice = dto.SellingPrice,
                            Barcode = dto.Barcode
                        };

                        await _itemGroupItemRepository.AddAsync(newItemGroupItem);
                        await _itemGroupItemRepository.SaveChanges();
                    }
                    else
                    {
                        existingItemGroupItem.SKU = dto.SKU;
                        existingItemGroupItem.PurchasePrice = dto.PurchasePrice;
                        existingItemGroupItem.SellingPrice = dto.SellingPrice;
                        existingItemGroupItem.Barcode = dto.Barcode;

                        await _itemGroupItemRepository.Update(existingItemGroupItem);
                    }
                }
                else
                {
                    var newItemGroupItem = new ItemGroupItem
                    {
                        Id = Guid.NewGuid(),
                        GroupId = dto.ItemGroupId.Value,
                        ProductId = existingProduct.Id,
                        SKU = dto.SKU,
                        PurchasePrice = dto.PurchasePrice,
                        SellingPrice = dto.SellingPrice,
                        Barcode = dto.Barcode
                    };

                    await _itemGroupItemRepository.AddAsync(newItemGroupItem);
                    await _itemGroupItemRepository.SaveChanges();
                }
            }
            else if (existingItemGroupItem != null)
            {
                await _itemGroupItemRepository.Delete(existingItemGroupItem.Id);
            }

            if (dto.TaxProfileIds != null && dto.TaxProfileIds.Any())
            {
                var existingTaxProfiles = await _productTaxProfileRepository
                    .Get(ptp => ptp.ProductId == existingProduct.Id)
                    .ToListAsync(cancellationToken);

                await _productTaxProfileRepository.DeleteRange(existingTaxProfiles);

                var productTaxProfiles = dto.TaxProfileIds.Select((taxProfileId, index) => new ProductTaxProfile
                {
                    ProductId = existingProduct.Id,
                    TaxProfileId = taxProfileId,
                    IsPrimary = index == 0
                }).ToList();

                await _productTaxProfileRepository.AddRangeAsync(productTaxProfiles);
                await _productTaxProfileRepository.SaveChangesAsync();
            }

            var updatedProduct = await _productRepository.GetByID(existingProduct.Id);

            if (updatedProduct == null)
                throw new NotFoundException("Product not found after update", Common.Enum.Finance_Enum.FinanceErrorCode.NotFound);

            var responseDto = await MapToDetailsDto(updatedProduct, cancellationToken);

            return ResponseViewModel<ProductDetailsDto>.Success(responseDto, "Product updated successfully");
        }

        private async Task ValidateProductUpdate(UpdateProductDto dto, Guid productId)
        {
            var errors = new Dictionary<string, string[]>();

            var skuExists = await _productRepository.AnyAsync(p =>
                p.SKU == dto.SKU &&
                p.CompanyId == dto.CompanyId &&
                p.Id != productId);

            if (skuExists)
            {
                errors.Add("SKU", new[] { "SKU already exists for this company" });
            }

            if (!string.IsNullOrEmpty(dto.Barcode))
            {
                var barcodeExists = await _productRepository.AnyAsync(p =>
                    p.Barcode == dto.Barcode &&
                    p.CompanyId == dto.CompanyId &&
                    p.Id != productId);

                if (barcodeExists)
                {
                    errors.Add("Barcode", new[] { "Barcode already exists for this company" });
                }
            }

            // Validate Warehouse
            var warehouseExists = await _warehouseRepository.AnyAsync(w =>
                w.Id == dto.WarehouseId &&
                w.CompanyId == dto.CompanyId &&
                w.Status == Common.Enum.Inventory_Enum.WarehouseStatus.Active);

            if (!warehouseExists)
            {
                errors.Add("WarehouseId", new[] { "Warehouse not found or not active" });
            }

            if (dto.MinimumPrice.HasValue && dto.SellingPrice < dto.MinimumPrice.Value)
            {
                errors.Add("SellingPrice", new[] { "Selling price cannot be lower than minimum price" });
            }

            if (errors.Any())
            {
                throw new ValidationException("Product validation failed", errors, "Inventory");
            }
        }

        private async Task<ProductDetailsDto> MapToDetailsDto(Product product, CancellationToken cancellationToken)
        {
            var responseDto = _mapper.Map<ProductDetailsDto>(product);
            responseDto.PhotoUrl = product.Photo;

            // Load Warehouse
            var warehouse = await _warehouseRepository.GetByID(product.WarehouseId);
            responseDto.WarehouseName = warehouse?.Name;

            if (product.CategoryId.HasValue)
            {
                var category = await _categoryRepository.GetByID(product.CategoryId.Value);
                responseDto.CategoryName = category?.Name ?? "N/A";
            }
            else
            {
                responseDto.CategoryName = "N/A";
            }

            if (product.BrandId.HasValue)
            {
                var brand = await _brandRepository.GetByID(product.BrandId.Value);
                responseDto.BrandName = brand?.Name;
            }

            if (product.SupplierId.HasValue)
            {
                var supplier = await _supplierRepository.GetByID(product.SupplierId.Value);
                responseDto.SupplierName = supplier?.Name;
            }

            var itemGroupItem = await _itemGroupItemRepository
                .Get(igi => igi.ProductId == product.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (itemGroupItem != null)
            {
                var itemGroup = await _itemGroupRepository.GetByID(itemGroupItem.GroupId);
                responseDto.ItemGroupId = itemGroup?.Id;
                responseDto.ItemGroupName = itemGroup?.Name;
            }

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

        private decimal? CalculateProfitMargin(decimal purchasePrice, decimal sellingPrice)
        {
            if (sellingPrice > 0 && purchasePrice > 0)
            {
                return (sellingPrice - purchasePrice) / sellingPrice * 100;
            }
            return null;
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