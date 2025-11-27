using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_SupplierPayments;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Modules.Purchases.Invoicing.Qeuries.Qeuries_SupplierPayments;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Invoicing.Handlers.Handlers_SupplierPayments
{
    public class GetSupplierPaymentByIdHandler : IRequestHandler<GetSupplierPaymentByIdQuery, ResponseViewModel<SupplierPaymentDto>>
    {
        private readonly IGeneralRepository<SupplierPayment> _paymentRepository;
        private readonly ILogger<GetSupplierPaymentByIdHandler> _logger;

        public GetSupplierPaymentByIdHandler(
            IGeneralRepository<SupplierPayment> paymentRepository,
            ILogger<GetSupplierPaymentByIdHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<SupplierPaymentDto>> Handle(
            GetSupplierPaymentByIdQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching supplier payment: {PaymentId}", request.Id);

            var payment = await _paymentRepository
                .Get(p => p.Id == request.Id && !p.IsDeleted)
                .Select(p => new SupplierPaymentDto
                {
                    Id = p.Id,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier.Name,
                    InvoiceId = p.InvoiceId,
                    InvoiceNumber = p.Invoice != null ? p.Invoice.InvoiceNumber : null,
                    PurchaseOrderId = p.PurchaseOrderId,
                    PONumber = p.PurchaseOrder != null ? p.PurchaseOrder.PONumber : null,
                    PaymentNumber = p.PaymentNumber,
                    PaymentType = p.PaymentType.ToString(),
                    PaymentMethodId = p.PaymentMethodId,
                    PaymentMethodName = p.PaymentMethod.Name,
                    PaymentMethodCode = p.PaymentMethod.Code,
                    PaymentMethodRequiresReference = p.PaymentMethod.RequiresReference,
                    PaymentDate = p.PaymentDate,
                    Amount = p.Amount,
                    AllocatedAmount = p.AllocatedAmount,
                    UnallocatedAmount = p.UnallocatedAmount,
                    ReferenceNumber = p.ReferenceNumber,
                    Notes = p.Notes,
                    Status = p.Status.ToString(),
                    IsVoid = p.IsVoid,
                    VoidedAt = p.VoidedAt,
                    VoidedByName = p.VoidedByUser != null ? p.VoidedByUser.UserName : null,
                    VoidReason = p.VoidReason,
                    CreatedAt = p.CreatedAt,
                    CreatedBy = p.CreatedBy ?? ""
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (payment == null)
            {
                _logger.LogWarning("Payment not found: {PaymentId}", request.Id);
                throw new NotFoundException(
                    $"Payment with ID {request.Id} not found",
                    Common.Enum.Finance_Enum.FinanceErrorCode.NotFound);
            }

            return ResponseViewModel<SupplierPaymentDto>.Success(
                payment,
                "Payment retrieved successfully");
        }
    }
}
