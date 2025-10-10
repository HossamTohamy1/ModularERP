using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_BulkDiscount;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handlers_BulkDiscount
{
    public class DeleteBulkDiscountCommandHandler : IRequestHandler<DeleteBulkDiscountCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<BulkDiscount> _repository;

        public DeleteBulkDiscountCommandHandler(IGeneralRepository<BulkDiscount> repository)
        {
            _repository = repository;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeleteBulkDiscountCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByID(request.Id);
            if (entity == null || entity.PriceListId != request.PriceListId)
            {
                throw new NotFoundException(
                    $"Bulk discount with ID {request.Id} not found in price list {request.PriceListId}.",
                    FinanceErrorCode.NotFound);
            }

            await _repository.Delete(request.Id);

            return ResponseViewModel<bool>.Success(true, "Bulk discount deleted successfully");
        }
    }
}
