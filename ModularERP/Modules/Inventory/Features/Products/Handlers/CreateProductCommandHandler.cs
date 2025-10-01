using AutoMapper;
using MediatR;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.Commends;
using ModularERP.Modules.Inventory.Features.Products.DTO;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Products.Handlers
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ResponseViewModel<Guid>>
    {
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IGeneralRepository<ProductStats> _statsRepository;
        private readonly IMapper _mapper;

        public CreateProductCommandHandler(
            IGeneralRepository<Product> productRepository,
            IGeneralRepository<ProductStats> statsRepository,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _statsRepository = statsRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var dto = request.ProductDto;

            // Validate business rules
            await ValidateProduct(dto);

            // Map DTO to Product entity
            var product = _mapper.Map<Product>(dto);
            product.Id = Guid.NewGuid();
            product.CreatedAt = DateTime.UtcNow;

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChanges();

            // Initialize product stats
            // في Handle method عند إنشاء ProductStats:
            if (dto.TrackStock)
            {
                var stats = new ProductStats
                {
                    ProductId = product.Id,
                    CompanyId = dto.CompanyId,  // بدلاً من product.CompanyId
                    OnHandStock = dto.InitialStock ?? 0,
                    TotalSold = 0,
                    SoldLast28Days = 0,
                    SoldLast7Days = 0,
                    AvgUnitCost = dto.PurchasePrice,
                    LastUpdated = DateTime.UtcNow
                };

                await _statsRepository.AddAsync(stats);
                await _statsRepository.SaveChanges();
            }

            return ResponseViewModel<Guid>.Success(product.Id, "Product created successfully");
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
    }
}