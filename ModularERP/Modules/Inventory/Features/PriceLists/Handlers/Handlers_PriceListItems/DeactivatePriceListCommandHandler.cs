using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceListItems;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handlers_PriceListItems
{
    public class DeactivatePriceListCommandHandler : IRequestHandler<DeactivatePriceListCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<PriceList> _repository;

        public DeactivatePriceListCommandHandler(IGeneralRepository<PriceList> repository)
        {
            _repository = repository;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeactivatePriceListCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var priceList = await _repository.GetByIDWithTracking(request.Id);
                if (priceList == null)
                {
                    throw new NotFoundException(
                        $"Price list with ID {request.Id} not found",
                        FinanceErrorCode.NotFound
                    );
                }

                if (priceList.Status == PriceListStatus.Inactive)
                {
                    throw new BusinessLogicException(
                        "Price list is already inactive",
                        "Inventory"
                    );
                }

                if (priceList.IsDefault)
                {
                    throw new BusinessLogicException(
                        "Cannot deactivate the default price list. Please set another price list as default first.",
                        "Inventory"
                    );
                }

                priceList.Status = PriceListStatus.Inactive;
                priceList.UpdatedAt = DateTime.UtcNow;

                await _repository.Update(priceList);

                return ResponseViewModel<bool>.Success(true, "Price list deactivated successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (BusinessLogicException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException(
                    $"Error deactivating price list: {ex.Message}",
                    "Inventory"
                );
            }
        }
    }
}
