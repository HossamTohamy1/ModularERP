using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.Commends.Commends_Product;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Products.Handlers.Handlers_Product
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IGeneralRepository<ProductStats> _statsRepository;
        private readonly IGeneralRepository<ItemGroupItem> _itemGroupItemRepository;
        private readonly IJoinTableRepository<ProductTaxProfile> _productTaxProfileRepository;

        public DeleteProductCommandHandler(
            IGeneralRepository<Product> productRepository,
            IGeneralRepository<ProductStats> statsRepository,
            IGeneralRepository<ItemGroupItem> itemGroupItemRepository,
            IJoinTableRepository<ProductTaxProfile> productTaxProfileRepository)
        {
            _productRepository = productRepository;
            _statsRepository = statsRepository;
            _itemGroupItemRepository = itemGroupItemRepository;
            _productTaxProfileRepository = productTaxProfileRepository;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByID(request.ProductId);

            if (product == null || product.CompanyId != request.CompanyId)
            {
                throw new NotFoundException(
                    "Product not found or does not belong to this company",
                    Common.Enum.Finance_Enum.FinanceErrorCode.NotFound);
            }

            var itemGroupItems = await _itemGroupItemRepository
                .Get(igi => igi.ProductId == request.ProductId)
                .ToListAsync(cancellationToken);

            foreach (var item in itemGroupItems)
            {
                await _itemGroupItemRepository.Delete(item.Id);
            }

            var productTaxProfiles = await _productTaxProfileRepository
                .Get(ptp => ptp.ProductId == request.ProductId)
                .ToListAsync(cancellationToken);

            if (productTaxProfiles.Any())
            {
                await _productTaxProfileRepository.DeleteRange(productTaxProfiles);
            }

            var stats = await _statsRepository
                .Get(s => s.ProductId == request.ProductId)
                .FirstOrDefaultAsync(cancellationToken);

            if (stats != null)
            {
                await _statsRepository.Delete(stats.Id);
            }

            await _productRepository.Delete(request.ProductId);

            return ResponseViewModel<bool>.Success(true, "Product deleted successfully");
        }
    }
}
