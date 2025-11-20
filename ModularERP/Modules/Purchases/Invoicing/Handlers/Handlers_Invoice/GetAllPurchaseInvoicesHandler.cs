using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_Invoice;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Modules.Purchases.Invoicing.Qeuries.Qeuires_Invoice;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Invoicing.Handlers.Handlers_Invoice
{
    public class GetAllPurchaseInvoicesHandler : IRequestHandler<GetAllPurchaseInvoicesQuery, ResponseViewModel<List<PurchaseInvoiceDto>>>
    {
        private readonly IGeneralRepository<PurchaseInvoice> _invoiceRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllPurchaseInvoicesHandler> _logger;

        public GetAllPurchaseInvoicesHandler(
            IGeneralRepository<PurchaseInvoice> invoiceRepository,
            IMapper mapper,
            ILogger<GetAllPurchaseInvoicesHandler> logger)
        {
            _invoiceRepository = invoiceRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<PurchaseInvoiceDto>>> Handle(
            GetAllPurchaseInvoicesQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Retrieving purchase invoices for company: {CompanyId}",
                    request.CompanyId);

                var query = _invoiceRepository.GetByCompanyId(request.CompanyId);

                // Apply filters
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(i =>
                        i.InvoiceNumber.Contains(request.SearchTerm));
                }

                if (!string.IsNullOrWhiteSpace(request.PaymentStatus))
                {
                    query = query.Where(i => i.PaymentStatus == request.PaymentStatus);
                }

                if (request.FromDate.HasValue)
                {
                    query = query.Where(i => i.InvoiceDate >= request.FromDate.Value);
                }

                if (request.ToDate.HasValue)
                {
                    query = query.Where(i => i.InvoiceDate <= request.ToDate.Value);
                }

                // Use AutoMapper projection to avoid N+1 queries
                var invoices = await query
                    .OrderByDescending(i => i.CreatedAt)
                    .ProjectTo<PurchaseInvoiceDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation(
                    "Retrieved {Count} purchase invoices for company: {CompanyId}",
                    invoices.Count, request.CompanyId);

                return ResponseViewModel<List<PurchaseInvoiceDto>>.Success(
                    invoices,
                    $"Retrieved {invoices.Count} purchase invoice(s)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving purchase invoices for company: {CompanyId}",
                    request.CompanyId);
                throw;
            }
        }
    }
}