using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceListAssignment;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handlers_PriceListAssignment
{
    public class DeletePriceListAssignmentHandler : IRequestHandler<DeletePriceListAssignmentCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<PriceListAssignment> _repository;

        public DeletePriceListAssignmentHandler(IGeneralRepository<PriceListAssignment> repository)
        {
            _repository = repository;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            DeletePriceListAssignmentCommand request,
            CancellationToken ct)
        {
            var entity = await _repository.GetByID(request.Id);

            if (entity == null)
            {
                throw new NotFoundException(
                    "Price list assignment not found",
                    FinanceErrorCode.NotFound);
            }

            await _repository.Delete(request.Id);

            return ResponseViewModel<bool>.Success(
                true,
                "Price list assignment deleted successfully");
        }
    }
}
