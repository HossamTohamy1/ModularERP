using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceListItems;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handlers_PriceListItems
{
    public class ExportPriceListCommandHandler : IRequestHandler<ExportPriceListCommand, ResponseViewModel<byte[]>>
    {
        private readonly IGeneralRepository<PriceList> _repository;
        private readonly IGeneralRepository<PriceListItem> _itemRepository;

        public ExportPriceListCommandHandler(
            IGeneralRepository<PriceList> repository,
            IGeneralRepository<PriceListItem> itemRepository)
        {
            _repository = repository;
            _itemRepository = itemRepository;
        }

        public async Task<ResponseViewModel<byte[]>> Handle(ExportPriceListCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var priceList = await _repository.GetByID(request.Id);
                if (priceList == null)
                {
                    throw new NotFoundException(
                        $"Price list with ID {request.Id} not found",
                        FinanceErrorCode.NotFound
                    );
                }

                // Get items with product/service details using projection
                var items = await _itemRepository
                    .GetAll()
                    .Where(x => x.PriceListId == request.Id)
                    .Select(x => new
                    {
                        ProductName = x.Product != null ? x.Product.Name : x.Service.Name,
                        ProductSKU = x.Product != null ? x.Product.SKU : x.Service.Code,
                        x.BasePrice,
                        x.ListPrice,
                        x.DiscountValue,
                        x.DiscountType,
                        x.FinalPrice,
                        x.ValidFrom,
                        x.ValidTo
                    })
                    .ToListAsync(cancellationToken);

                // Create CSV content
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("Product Name,SKU/Code,Base Price,List Price,Discount Value,Discount Type,Final Price,Valid From,Valid To");

                foreach (var item in items)
                {
                    csv.AppendLine($"{item.ProductName},{item.ProductSKU},{item.BasePrice},{item.ListPrice},{item.DiscountValue},{item.DiscountType},{item.FinalPrice},{item.ValidFrom},{item.ValidTo}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());

                return ResponseViewModel<byte[]>.Success(bytes, "Price list exported successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException(
                    $"Error exporting price list: {ex.Message}",
                    "Inventory"
                );
            }
        }
    }
}
