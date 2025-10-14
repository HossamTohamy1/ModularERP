using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_RequisitionItem;
using ModularERP.Modules.Inventory.Features.Requisitions.Models;
using ModularERP.Shared.Interfaces;
using Serilog;
using ILogger = Serilog.ILogger;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Handlers.Handlers_RequisitionItem
{
    public class DeleteRequisitionItemHandler : IRequestHandler<DeleteRequisitionItemCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<RequisitionItem> _repository;
        private readonly IGeneralRepository<Requisition> _requisitionRepository;
        private readonly ILogger _logger;

        public DeleteRequisitionItemHandler(
            IGeneralRepository<RequisitionItem> repository,
            IGeneralRepository<Requisition> requisitionRepository)
        {
            _repository = repository;
            _requisitionRepository = requisitionRepository;
            _logger = Log.ForContext<DeleteRequisitionItemHandler>();
        }

        public async Task<ResponseViewModel<bool>> Handle(DeleteRequisitionItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.Information("Deleting item {ItemId} from requisition {RequisitionId}",
                    request.ItemId, request.RequisitionId);

                // التحقق من وجود الـ Item
                var item = await _repository.GetByID(request.ItemId);
                if (item == null || item.RequisitionId != request.RequisitionId)
                {
                    _logger.Warning("Item {ItemId} not found in requisition {RequisitionId}",
                        request.ItemId, request.RequisitionId);
                    throw new NotFoundException(
                        $"Item with ID {request.ItemId} not found in requisition {request.RequisitionId}",
                        FinanceErrorCode.NotFound
                    );
                }

                // التحقق من حالة الـ Requisition
                var requisition = await _requisitionRepository.GetByID(request.RequisitionId);
                if (requisition == null)
                {
                    _logger.Warning("Requisition {RequisitionId} not found", request.RequisitionId);
                    throw new NotFoundException(
                        $"Requisition with ID {request.RequisitionId} not found",
                        FinanceErrorCode.NotFound
                    );
                }

                if (requisition.Status != RequisitionStatus.Draft)
                {
                    _logger.Warning("Cannot delete items from requisition {RequisitionId} with status {Status}",
                        request.RequisitionId, requisition.Status);
                    throw new BusinessLogicException(
                        "Cannot delete items from a requisition that is not in Draft status",
                        "Inventory",
                        FinanceErrorCode.BusinessLogicError
                    );
                }

                // حذف الـ Item (Soft Delete)
                await _repository.Delete(request.ItemId);

                _logger.Information("Item {ItemId} deleted successfully from requisition {RequisitionId}",
                    request.ItemId, request.RequisitionId);

                return ResponseViewModel<bool>.Success(
                    true,
                    "Requisition item deleted successfully"
                );
            }
            catch (Exception ex) when (ex is not BusinessLogicException && ex is not NotFoundException)
            {
                _logger.Error(ex, "Error deleting item {ItemId} from requisition {RequisitionId}",
                    request.ItemId, request.RequisitionId);
                throw;
            }
        }
    }
}