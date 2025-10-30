using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POStatus;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Qeuries.Qeuries_POStauts;
using ModularERP.Modules.Purchases.WorkFlow.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_POStatus
{
    public class GetPOTimelineHandler : IRequestHandler<GetPOTimelineQuery, ResponseViewModel<POTimelineDto>>
    {
        private readonly IGeneralRepository<PurchaseOrder> _poRepo;
        private readonly IGeneralRepository<POAuditLog> _auditRepo;
        private readonly ILogger<GetPOTimelineHandler> _logger;

        public GetPOTimelineHandler(
            IGeneralRepository<PurchaseOrder> poRepo,
            IGeneralRepository<POAuditLog> auditRepo,
            ILogger<GetPOTimelineHandler> logger)
        {
            _poRepo = poRepo;
            _auditRepo = auditRepo;
            _logger = logger;
        }

        public async Task<ResponseViewModel<POTimelineDto>> Handle(
            GetPOTimelineQuery request,
            CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Building timeline for PO ID: {POId}", request.PurchaseOrderId);

                var po = await _poRepo.GetByID(request.PurchaseOrderId);
                if (po == null)
                {
                    throw new NotFoundException(
                        $"Purchase Order with ID {request.PurchaseOrderId} not found",
                        FinanceErrorCode.NotFound);
                }

                var events = new List<TimelineEventDto>();

                // Created Event
                events.Add(new TimelineEventDto
                {
                    EventDate = po.CreatedAt,
                    EventType = "Created",
                    Title = "Purchase Order Created",
                    Description = $"PO #{po.PONumber} was created",
                    Icon = "file-plus",
                    Status = "completed"
                });

                // Submitted Event
                if (po.SubmittedAt.HasValue)
                {
                    events.Add(new TimelineEventDto
                    {
                        EventDate = po.SubmittedAt.Value,
                        EventType = "Submitted",
                        Title = "Submitted for Approval",
                        Description = "PO submitted to supplier",
                        Icon = "send",
                        Status = "completed"
                    });
                }

                // Approved Event
                if (po.ApprovedAt.HasValue)
                {
                    events.Add(new TimelineEventDto
                    {
                        EventDate = po.ApprovedAt.Value,
                        EventType = "Approved",
                        Title = "PO Approved",
                        Description = "Purchase order has been approved",
                        Icon = "check-circle",
                        Status = "completed"
                    });
                }

                // Reception Events
                if (po.ReceptionStatus != "NotReceived")
                {
                    events.Add(new TimelineEventDto
                    {
                        EventDate = po.UpdatedAt ?? po.CreatedAt,
                        EventType = "Received",
                        Title = $"Goods {po.ReceptionStatus}",
                        Description = $"Reception status: {po.ReceptionStatus}",
                        Icon = "package",
                        Status = po.ReceptionStatus == "FullyReceived" ? "completed" : "in-progress"
                    });
                }

                // Payment Events
                if (po.PaymentStatus != "Unpaid")
                {
                    events.Add(new TimelineEventDto
                    {
                        EventDate = po.UpdatedAt ?? po.CreatedAt,
                        EventType = "Payment",
                        Title = $"Payment {po.PaymentStatus}",
                        Description = $"Payment status: {po.PaymentStatus}",
                        Icon = "dollar-sign",
                        Status = po.PaymentStatus == "PaidInFull" ? "completed" : "in-progress"
                    });
                }

                // Closed Event
                if (po.ClosedAt.HasValue)
                {
                    events.Add(new TimelineEventDto
                    {
                        EventDate = po.ClosedAt.Value,
                        EventType = "Closed",
                        Title = "PO Closed",
                        Description = "Purchase order completed and closed",
                        Icon = "check-square",
                        Status = "completed"
                    });
                }

                var timeline = new POTimelineDto
                {
                    PurchaseOrderId = po.Id,
                    PONumber = po.PONumber,
                    Events = events.OrderBy(e => e.EventDate).ToList()
                };

                _logger.LogInformation("Timeline built with {Count} events", timeline.Events.Count);
                return ResponseViewModel<POTimelineDto>.Success(timeline, "Timeline retrieved successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building timeline for PO ID: {POId}", request.PurchaseOrderId);
                throw new BusinessLogicException(
                    "Error retrieving purchase order timeline",
                    ex,
                    "PurchaseOrders",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }
}
