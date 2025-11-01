using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POStatus;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Qeuries.Qeuries_POStauts;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_POStatus
{
    public class GetPOTimelineHandler : IRequestHandler<GetPOTimelineQuery, ResponseViewModel<POTimelineDto>>
    {
        private readonly IGeneralRepository<PurchaseOrder> _poRepo;
        private readonly ILogger<GetPOTimelineHandler> _logger;

        public GetPOTimelineHandler(
            IGeneralRepository<PurchaseOrder> poRepo,
            ILogger<GetPOTimelineHandler> logger)
        {
            _poRepo = poRepo;
            _logger = logger;
        }

        public async Task<ResponseViewModel<POTimelineDto>> Handle(
            GetPOTimelineQuery request,
            CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Building timeline for PO ID: {POId}", request.PurchaseOrderId);

                var po = await _poRepo.GetAll()
                    .Include(p => p.ApprovedByUser)
                    .Where(p => p.Id == request.PurchaseOrderId && !p.IsDeleted)
                    .Select(p => new
                    {
                        p.Id,
                        p.PONumber,
                        p.CreatedAt,
                        p.CreatedById,
                        p.SubmittedAt,
                        p.ApprovedAt,
                        p.ApprovedBy,
                        ApprovedByName = p.ApprovedByUser != null 
                            ? (p.ApprovedByUser.FirstName ?? "") + " " + (p.ApprovedByUser.LastName ?? "")
                            : null,
                        p.ClosedAt,
                        p.DocumentStatus,
                        p.PaymentStatus,
                        p.ReceptionStatus,
                        p.UpdatedAt,
                        TotalAmount = p.LineItems
                            .Where(li => !li.IsDeleted)
                            .Sum(li => li.LineTotal),
                        Deposits = p.Deposits
                            .Where(d => !d.IsDeleted)
                            .Select(d => new
                            {
                                d.Id,
                                d.Amount,
                                d.Percentage,
                                d.AlreadyPaid,
                                d.PaymentDate,
                                d.PaymentMethod,
                                d.ReferenceNumber,
                                d.CreatedAt,
                                d.CreatedById,
                                d.UpdatedAt
                            })
                            .OrderBy(d => d.CreatedAt)
                            .ToList()
                    })
                    .FirstOrDefaultAsync(ct);

                if (po == null)
                {
                    throw new NotFoundException(
                        $"Purchase Order with ID {request.PurchaseOrderId} not found",
                        FinanceErrorCode.NotFound);
                }

                var events = new List<TimelineEventDto>();

                // ✅ 1. PO Created Event
                events.Add(new TimelineEventDto
                {
                    EventDate = po.CreatedAt,
                    EventType = "Created",
                    Title = "Purchase Order Created",
                    Description = $"PO #{po.PONumber} was created with total amount of {po.TotalAmount:N2} EGP",
                    PerformedBy = "System",
                    Icon = "file-plus",
                    Status = "completed"
                });

                // ✅ 2. Submitted Event (if submitted)
                if (po.SubmittedAt.HasValue)
                {
                    events.Add(new TimelineEventDto
                    {
                        EventDate = po.SubmittedAt.Value,
                        EventType = "Submitted",
                        Title = "Purchase Order Submitted",
                        Description = $"PO #{po.PONumber} submitted to supplier for processing",
                        PerformedBy = "System",
                        Icon = "send",
                        Status = "completed"
                    });
                }

                // ✅ 3. Approved Event (if approved)
                if (po.ApprovedAt.HasValue)
                {
                    events.Add(new TimelineEventDto
                    {
                        EventDate = po.ApprovedAt.Value,
                        EventType = "Approved",
                        Title = "Purchase Order Approved",
                        Description = $"PO #{po.PONumber} has been approved and ready for execution",
                        PerformedBy = !string.IsNullOrWhiteSpace(po.ApprovedByName) 
                            ? po.ApprovedByName.Trim() 
                            : "System",
                        Icon = "check-circle",
                        Status = "completed"
                    });
                }

                // ✅ 4. Deposit Events (each deposit as separate event)
                foreach (var deposit in po.Deposits)
                {
                    var depositTitle = deposit.AlreadyPaid
                        ? "Deposit Payment Received"
                        : "Deposit Recorded";

                    var depositDescription = $"Amount: {deposit.Amount:N2} EGP " +
                                           $"({deposit.Percentage:N2}%) " +
                                           $"via {deposit.PaymentMethod}";

                    if (!string.IsNullOrEmpty(deposit.ReferenceNumber))
                    {
                        depositDescription += $" - Ref: {deposit.ReferenceNumber}";
                    }

                    if (!deposit.AlreadyPaid)
                    {
                        depositDescription += " - Payment pending";
                    }

                    events.Add(new TimelineEventDto
                    {
                        EventDate = deposit.PaymentDate ?? deposit.CreatedAt,
                        EventType = "Deposit",
                        Title = depositTitle,
                        Description = depositDescription,
                        PerformedBy = "System",
                        Icon = deposit.AlreadyPaid ? "dollar-sign" : "clock",
                        Status = deposit.AlreadyPaid ? "completed" : "pending"
                    });

                    // ✅ Add update event if deposit was modified
                    if (deposit.UpdatedAt.HasValue && deposit.UpdatedAt > deposit.CreatedAt.AddMinutes(1))
                    {
                        events.Add(new TimelineEventDto
                        {
                            EventDate = deposit.UpdatedAt.Value,
                            EventType = "DepositUpdated",
                            Title = "Deposit Updated",
                            Description = $"Deposit details updated - Amount: {deposit.Amount:N2} EGP",
                            PerformedBy = "System",
                            Icon = "edit",
                            Status = "completed"
                        });
                    }
                }

                // ✅ 5. Payment Status Summary Event
                if (po.Deposits.Any(d => d.AlreadyPaid))
                {
                    var totalPaid = po.Deposits.Where(d => d.AlreadyPaid).Sum(d => d.Amount);
                    var paidPercentage = po.TotalAmount > 0
                        ? Math.Round((totalPaid / po.TotalAmount) * 100, 2)
                        : 0;

                    var paymentStatusTitle = po.PaymentStatus switch
                    {
                        "Paid in Full" => "Payment Completed",
                        "Partially Paid" => "Partial Payment Received",
                        _ => "Payment Status Updated"
                    };

                    var paymentStatusDescription = $"Total paid: {totalPaid:N2} EGP ({paidPercentage}%) " +
                                                  $"- Remaining: {(po.TotalAmount - totalPaid):N2} EGP";

                    var lastPaymentDate = po.Deposits
                        .Where(d => d.AlreadyPaid)
                        .OrderByDescending(d => d.PaymentDate ?? d.CreatedAt)
                        .FirstOrDefault()?.PaymentDate ?? po.UpdatedAt ?? DateTime.UtcNow;

                    events.Add(new TimelineEventDto
                    {
                        EventDate = lastPaymentDate,
                        EventType = "PaymentStatus",
                        Title = paymentStatusTitle,
                        Description = paymentStatusDescription,
                        PerformedBy = "System",
                        Icon = po.PaymentStatus == "Paid in Full" ? "check-circle" : "dollar-sign",
                        Status = po.PaymentStatus == "Paid in Full" ? "completed" : "in-progress"
                    });
                }

                // ✅ 6. Reception Status Events
                if (po.ReceptionStatus != "Not Received" && po.ReceptionStatus != "NotReceived")
                {
                    var receptionTitle = po.ReceptionStatus switch
                    {
                        "Fully Received" or "FullyReceived" => "Goods Fully Received",
                        "Partially Received" or "PartiallyReceived" => "Goods Partially Received",
                        "Returned" => "Goods Returned",
                        _ => "Reception Status Updated"
                    };

                    var receptionDescription = po.ReceptionStatus switch
                    {
                        "Fully Received" or "FullyReceived" =>
                            "All items have been received and inspected",
                        "Partially Received" or "PartiallyReceived" =>
                            "Some items received - awaiting remaining delivery",
                        "Returned" =>
                            "Items have been returned to supplier",
                        _ => $"Reception status: {po.ReceptionStatus}"
                    };

                    events.Add(new TimelineEventDto
                    {
                        EventDate = po.UpdatedAt ?? po.CreatedAt,
                        EventType = "Reception",
                        Title = receptionTitle,
                        Description = receptionDescription,
                        PerformedBy = "System",
                        Icon = "package",
                        Status = po.ReceptionStatus == "Fully Received" || po.ReceptionStatus == "FullyReceived"
                            ? "completed"
                            : "in-progress"
                    });
                }

                // ✅ 7. Closed Event (if PO is closed)
                if (po.ClosedAt.HasValue)
                {
                    events.Add(new TimelineEventDto
                    {
                        EventDate = po.ClosedAt.Value,
                        EventType = "Closed",
                        Title = "Purchase Order Closed",
                        Description = $"PO #{po.PONumber} has been completed and closed - " +
                                    $"All items received and payments settled",
                        PerformedBy = "System",
                        Icon = "check-square",
                        Status = "completed"
                    });
                }

                // ✅ 8. Cancelled Event (if PO is cancelled)
                if (po.DocumentStatus == "Cancelled")
                {
                    events.Add(new TimelineEventDto
                    {
                        EventDate = po.UpdatedAt ?? DateTime.UtcNow,
                        EventType = "Cancelled",
                        Title = "Purchase Order Cancelled",
                        Description = $"PO #{po.PONumber} has been cancelled - No further actions allowed",
                        PerformedBy = "System",
                        Icon = "x-circle",
                        Status = "cancelled"
                    });
                }

                // ✅ Sort events by date (oldest first)
                var sortedEvents = events.OrderBy(e => e.EventDate).ToList();

                var timeline = new POTimelineDto
                {
                    PurchaseOrderId = po.Id,
                    PONumber = po.PONumber,
                    Events = sortedEvents
                };

                _logger.LogInformation(
                    "Timeline built successfully for PO #{PONumber} with {Count} events",
                    po.PONumber, sortedEvents.Count);

                return ResponseViewModel<POTimelineDto>.Success(
                    timeline,
                    "Timeline retrieved successfully");
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