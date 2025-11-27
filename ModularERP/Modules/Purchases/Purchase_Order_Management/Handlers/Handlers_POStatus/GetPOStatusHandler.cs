using AutoMapper;
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
    public class GetPOStatusHandler : IRequestHandler<GetPOStatusQuery, ResponseViewModel<POStatusDto>>
    {
        private readonly IGeneralRepository<PurchaseOrder> _repo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPOStatusHandler> _logger;

        public GetPOStatusHandler(
            IGeneralRepository<PurchaseOrder> repo,
            IMapper mapper,
            ILogger<GetPOStatusHandler> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<POStatusDto>> Handle(
            GetPOStatusQuery request,
            CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Fetching PO status for ID: {POId}", request.PurchaseOrderId);

                // ✅ Use Select instead of ProjectTo for better control
                var status = await _repo.GetAll()
                    .Where(x => x.Id == request.PurchaseOrderId && !x.IsDeleted)
                    .Select(po => new POStatusDto
                    {
                        PurchaseOrderId = po.Id,
                        PONumber = po.PONumber,
                        SupplierName = po.Supplier.Name ?? "",
                        ReceptionStatus = po.ReceptionStatus.ToString() ?? "Not Received",
                        PaymentStatus = po.PaymentStatus.ToString() ?? "Unpaid",
                        DocumentStatus = po.DocumentStatus.ToString() ?? "Draft",
                        PODate = po.CreatedAt,
                        ApprovedAt = po.ApprovedAt,
                        ClosedAt = po.ClosedAt,

                        // ✅ Calculate Total Amount
                        TotalAmount = po.LineItems
                            .Where(li => !li.IsDeleted)
                            .Sum(li => li.LineTotal),

                        // ✅ Calculate Paid Amount
                        PaidPercentage = po.LineItems
                            .Where(li => !li.IsDeleted)
                            .Sum(li => li.LineTotal) > 0
                                ? Math.Round(
                                    (po.Deposits
                                        .Where(d => d.AlreadyPaid && !d.IsDeleted)
                                        .Sum(d => d.Amount)
                                    / po.LineItems
                                        .Where(li => !li.IsDeleted)
                                        .Sum(li => li.LineTotal)) * 100, 2)
                                : 0,

                        // ✅ Calculate Amount Due
                        AmountDue = po.LineItems
                            .Where(li => !li.IsDeleted)
                            .Sum(li => li.LineTotal)
                            - po.Deposits
                                .Where(d => d.AlreadyPaid && !d.IsDeleted)
                                .Sum(d => d.Amount),

                        // ✅ Calculate Received Percentage
                        ReceivedPercentage = po.LineItems
                            .Where(li => !li.IsDeleted)
                            .Sum(li => li.Quantity) > 0
                                ? Math.Round(
                                    (po.LineItems
                                        .Where(li => !li.IsDeleted)
                                        .Sum(li => li.ReceivedQuantity)
                                    / po.LineItems
                                        .Where(li => !li.IsDeleted)
                                        .Sum(li => li.Quantity)) * 100, 2)
                                : 0
                    })
                    .FirstOrDefaultAsync(ct);

                if (status == null)
                {
                    throw new NotFoundException(
                        $"Purchase Order with ID {request.PurchaseOrderId} not found",
                        FinanceErrorCode.NotFound);
                }

                // ✅ Update Payment Status based on PaidPercentage
                if (status.PaidPercentage >= 100)
                {
                    status.PaymentStatus = "Paid in Full";
                }
                else if (status.PaidPercentage > 0)
                {
                    status.PaymentStatus = "Partially Paid";
                }
                else
                {
                    status.PaymentStatus = "Unpaid";
                }

                // ✅ Cap PaidPercentage at 100
                status.PaidPercentage = Math.Min(status.PaidPercentage, 100);

                // ✅ Update Reception Status based on ReceivedPercentage
                if (status.ReceivedPercentage >= 100)
                {
                    status.ReceptionStatus = "Fully Received";
                }
                else if (status.ReceivedPercentage > 0)
                {
                    status.ReceptionStatus = "Partially Received";
                }
                else
                {
                    status.ReceptionStatus = "Not Received";
                }

                _logger.LogInformation(
                    "Successfully retrieved PO status for {PONumber}: " +
                    "Total={Total}, Paid={Paid} ({PaidPct}%), Due={Due}, " +
                    "Payment={PaymentStatus}, Reception={ReceptionStatus}",
                    status.PONumber, status.TotalAmount,
                    status.TotalAmount - status.AmountDue, status.PaidPercentage,
                    status.AmountDue, status.PaymentStatus, status.ReceptionStatus);

                return ResponseViewModel<POStatusDto>.Success(
                    status,
                    "PO status retrieved successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving PO status for ID: {POId}", request.PurchaseOrderId);
                throw new BusinessLogicException(
                    "Error retrieving purchase order status",
                    ex,
                    "PurchaseOrders",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }
}