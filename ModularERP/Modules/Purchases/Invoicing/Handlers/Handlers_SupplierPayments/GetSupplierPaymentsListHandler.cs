using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using ModularERP.Common.ViewModel;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_SupplierPayments;
using ModularERP.Modules.Purchases.Invoicing.Qeuries.Qeuries_SupplierPayments;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Common.Enum.Purchases_Enum;
using ModularERP.Common.Exceptions;

namespace ModularERP.Modules.Purchases.Invoicing.Handlers.Handlers_SupplierPayments
{
    public class GetSupplierPaymentsListHandler
        : IRequestHandler<GetSupplierPaymentsListQuery, ResponseViewModel<PagedResult<SupplierPaymentDto>>>
    {
        private readonly FinanceDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<GetSupplierPaymentsListHandler> _logger;

        public GetSupplierPaymentsListHandler(
            FinanceDbContext context,
            IMapper mapper,
            ILogger<GetSupplierPaymentsListHandler> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PagedResult<SupplierPaymentDto>>> Handle(
            GetSupplierPaymentsListQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var query = _context.SupplierPayments
                    .Include(sp => sp.Supplier)
                    .Include(sp => sp.Invoice)
                    .Include(sp => sp.PurchaseOrder)
                   // .Include(sp => sp.CreatedBy)
                    .Include(sp => sp.VoidedByUser)
                    .Where(sp => !sp.IsDeleted)
                    .AsQueryable();

                // Apply filters
                if (request.SupplierId.HasValue)
                    query = query.Where(sp => sp.SupplierId == request.SupplierId.Value);

                if (request.InvoiceId.HasValue)
                    query = query.Where(sp => sp.InvoiceId == request.InvoiceId.Value);

                if (request.PurchaseOrderId.HasValue)
                    query = query.Where(sp => sp.PurchaseOrderId == request.PurchaseOrderId.Value);

                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    if (Enum.TryParse<SupplierPaymentStatus>(request.Status, true, out var parsedStatus))
                    {
                        query = query.Where(sp => sp.Status == parsedStatus);
                    }
                    else
                    {
                        throw new BusinessLogicException(
                            $"Invalid SupplierPaymentStatus value: {request.Status}",
                            "Purchases");
                    }
                }


                if (request.FromDate.HasValue)
                    query = query.Where(sp => sp.PaymentDate >= request.FromDate.Value);

                if (request.ToDate.HasValue)
                    query = query.Where(sp => sp.PaymentDate <= request.ToDate.Value.Date.AddDays(1).AddTicks(-1));

                // Get total count before pagination
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply sorting (default: newest first)
                query = query.OrderByDescending(sp => sp.PaymentDate)
                            .ThenByDescending(sp => sp.CreatedAt);

                // Apply pagination
                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);

                // Map to DTOs
                var dtos = _mapper.Map<List<SupplierPaymentDto>>(items);

                var pagedResult = new PagedResult<SupplierPaymentDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ResponseViewModel<PagedResult<SupplierPaymentDto>>.Success(
                    pagedResult,
                    "Supplier payments retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplier payments list");
                return ResponseViewModel<PagedResult<SupplierPaymentDto>>.Error(
                    "An error occurred while retrieving supplier payments",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}