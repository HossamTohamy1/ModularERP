using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commenads_POStauts;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_POStatus
{
    public class UpdatePONotesHandler : IRequestHandler<UpdatePONotesCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<PurchaseOrder> _repo;
        private readonly ILogger<UpdatePONotesHandler> _logger;

        public UpdatePONotesHandler(
            IGeneralRepository<PurchaseOrder> repo,
            ILogger<UpdatePONotesHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            UpdatePONotesCommand request,
            CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Updating notes for PO ID: {POId}", request.PurchaseOrderId);

                var po = await _repo.GetByIDWithTracking(request.PurchaseOrderId);
                if (po == null)
                {
                    throw new NotFoundException(
                        $"Purchase Order with ID {request.PurchaseOrderId} not found",
                        FinanceErrorCode.NotFound);
                }

                if (!string.IsNullOrWhiteSpace(request.Notes))
                    po.Notes = request.Notes;

                if (!string.IsNullOrWhiteSpace(request.Terms))
                    po.Terms = request.Terms;

                po.UpdatedAt = DateTime.UtcNow;

                await _repo.SaveChanges();

                _logger.LogInformation("Successfully updated notes for PO {PONumber}", po.PONumber);
                return ResponseViewModel<bool>.Success(true, "Notes updated successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notes for PO ID: {POId}", request.PurchaseOrderId);
                throw new BusinessLogicException(
                    "Error updating purchase order notes",
                    ex,
                    "PurchaseOrders",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }
}
