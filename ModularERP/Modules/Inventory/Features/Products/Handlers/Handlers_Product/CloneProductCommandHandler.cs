using AutoMapper;
using MediatR;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.Commends.Commends_Product;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_Product;
using ModularERP.Modules.Inventory.Features.Products.DTO;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;
using ModularERP.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ModularERP.Modules.Inventory.Features.Products.Handlers.Handlers_Product
{
    public class CloneProductCommandHandler : IRequestHandler<CloneProductCommand, ResponseViewModel<ProductDetailsDto>>
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

        public CloneProductCommandHandler(
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

        public async Task<ResponseViewModel<ProductDetailsDto>> Handle(CloneProductCommand request, CancellationToken cancellationToken)
        {
            var sourceProduct = await _productRepository.GetByID(request.ProductId);

            if (sourceProduct == null || sourceProduct.CompanyId != request.CompanyId)
            {
                throw new NotFoundException(
                    "Product not found or does not belong to this company",
                    Common.Enum.Finance_Enum.FinanceErrorCode.NotFound);
            }

            var newSKU = await GenerateUniqueSKU(sourceProduct.SKU, request.CompanyId);

            var clonedProduct = new Product
            {
                Id = Guid.NewGuid(),
                Name = $"{sourceProduct.Name} (Copy)",
                SKU = newSKU,
                Description = sourceProduct.Description,
                Photo = sourceProduct.Photo,
                CompanyId = sourceProduct.CompanyId,
                CategoryId = sourceProduct.CategoryId,
                BrandId = sourceProduct.BrandId,
                SupplierId = sourceProduct.SupplierId,
                Barcode = null,
                PurchasePrice = sourceProduct.PurchasePrice,
                SellingPrice = sourceProduct.SellingPrice,
                MinPrice = sourceProduct.MinPrice,
                Discount = sourceProduct.Discount,
                DiscountType = sourceProduct.DiscountType,
                ProfitMargin = sourceProduct.ProfitMargin,
                TrackStock = sourceProduct.TrackStock,
                InitialStock = 0,
                LowStockThreshold = sourceProduct.LowStockThreshold,
                InternalNotes = sourceProduct.InternalNotes,
                Tags = sourceProduct.Tags,
                Status = Common.Enum.Inventory_Enum.ProductStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            await _productRepository.AddAsync(clonedProduct);
            await _productRepository.SaveChanges();

            if (sourceProduct.TrackStock)
            {
                var stats = new ProductStats
                {
                    ProductId = clonedProduct.Id,
                    CompanyId = sourceProduct.CompanyId,
                    OnHandStock = 0,
                    TotalSold = 0,
                    SoldLast28Days = 0,
                    SoldLast7Days = 0,
                    AvgUnitCost = sourceProduct.PurchasePrice ?? 0,
                    LastUpdated = DateTime.UtcNow
                };

                await _statsRepository.AddAsync(stats);
                await _statsRepository.SaveChanges();
            }

            var sourceItemGroupItem = await _itemGroupItemRepository
                .Get(igi => igi.ProductId == sourceProduct.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (sourceItemGroupItem != null)
            {
                var clonedItemGroupItem = new ItemGroupItem
                {
                    Id = Guid.NewGuid(),
                    GroupId = sourceItemGroupItem.GroupId,
                    ProductId = clonedProduct.Id,
                    SKU = newSKU,
                    PurchasePrice = sourceItemGroupItem.PurchasePrice,
                    SellingPrice = sourceItemGroupItem.SellingPrice,
                    Barcode = null
                };

                await _itemGroupItemRepository.AddAsync(clonedItemGroupItem);
                await _itemGroupItemRepository.SaveChanges();
            }

            var sourceTaxProfiles = await _productTaxProfileRepository
                .Get(ptp => ptp.ProductId == sourceProduct.Id)
                .ToListAsync(cancellationToken);

            if (sourceTaxProfiles.Any())
            {
                var clonedTaxProfiles = sourceTaxProfiles.Select(stp => new ProductTaxProfile
                {
                    ProductId = clonedProduct.Id,
                    TaxProfileId = stp.TaxProfileId,
                    IsPrimary = stp.IsPrimary
                }).ToList();

                await _productTaxProfileRepository.AddRangeAsync(clonedTaxProfiles);
                await _productTaxProfileRepository.SaveChangesAsync();
            }

            var createdProduct = await _productRepository.GetByID(clonedProduct.Id);

            if (createdProduct == null)
                throw new NotFoundException("Product not found after cloning", Common.Enum.Finance_Enum.FinanceErrorCode.NotFound);

            var responseDto = _mapper.Map<ProductDetailsDto>(createdProduct);
            responseDto.PhotoUrl = createdProduct.Photo;

            if (createdProduct.CategoryId.HasValue)
            {
                var category = await _categoryRepository.GetByID(createdProduct.CategoryId.Value);
                responseDto.CategoryName = category?.Name ?? "N/A";
            }
            else
            {
                responseDto.CategoryName = "N/A";
            }

            if (createdProduct.BrandId.HasValue)
            {
                var brand = await _brandRepository.GetByID(createdProduct.BrandId.Value);
                responseDto.BrandName = brand?.Name;
            }

            if (createdProduct.SupplierId.HasValue)
            {
                var supplier = await _supplierRepository.GetByID(createdProduct.SupplierId.Value);
                responseDto.SupplierName = supplier?.Name;
            }

            var clonedItemGroup = await _itemGroupItemRepository
                .Get(igi => igi.ProductId == createdProduct.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (clonedItemGroup != null)
            {
                var itemGroup = await _itemGroupRepository.GetByID(clonedItemGroup.GroupId);
                responseDto.ItemGroupId = itemGroup?.Id;
                responseDto.ItemGroupName = itemGroup?.Name;
            }

            if (createdProduct.TrackStock)
            {
                var stats = await _statsRepository
                    .Get(s => s.ProductId == createdProduct.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (stats != null)
                {
                    responseDto.Stats = _mapper.Map<ProductStatsDto>(stats);
                    responseDto.Stats.StockStatus = "OutOfStock";
                }
            }

            return ResponseViewModel<ProductDetailsDto>.Success(responseDto, "Product cloned successfully");
        }

        private async Task<string> GenerateUniqueSKU(string? baseSKU, Guid companyId)
        {
            var newSKU = $"{baseSKU}-COPY";
            var counter = 1;

            while (await _productRepository.AnyAsync(p =>
                p.SKU == newSKU &&
                p.CompanyId == companyId))
            {
                newSKU = $"{baseSKU}-COPY-{counter}";
                counter++;
            }

            return newSKU;
        }
    }
}
