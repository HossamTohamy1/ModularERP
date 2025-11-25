using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Modules.Purchases.KPIs.DTO;
using ModularERP.Modules.Purchases.KPIs.Qeuries;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.KPIs.Handlers
{
    public class GetPaymentTrendsHandler : IRequestHandler<GetPaymentTrendsQuery, ResponseViewModel<PaymentTrendsDto>>
    {
        private readonly IGeneralRepository<SupplierPayment> _paymentRepository;
        private readonly IGeneralRepository<PurchaseInvoice> _invoiceRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPaymentTrendsHandler> _logger;

        public GetPaymentTrendsHandler(
            IGeneralRepository<SupplierPayment> paymentRepository,
            IGeneralRepository<PurchaseInvoice> invoiceRepository,
            IMapper mapper,
            ILogger<GetPaymentTrendsHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _invoiceRepository = invoiceRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PaymentTrendsDto>> Handle(
            GetPaymentTrendsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching payment trends KPI for CompanyId: {CompanyId}", request.CompanyId);

                var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-12);
                var endDate = request.EndDate ?? DateTime.UtcNow;

                // Payment data
                var payments = await _paymentRepository.GetAll()
                    .Where(p => p.PaymentDate >= startDate &&
                                p.PaymentDate <= endDate &&
                                p.Status == "Posted" &&
                                !p.IsVoid)
                    .Select(p => new
                    {
                        p.Amount,
                        p.PaymentDate
                    })
                    .ToListAsync(cancellationToken);

                var totalPaid = payments.Sum(p => p.Amount);

                // Invoice data
                var invoices = await _invoiceRepository.GetByCompanyId(request.CompanyId)
                    .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate)
                    .Select(i => new
                    {
                        i.AmountDue,
                        i.TotalAmount,
                        i.PaymentStatus,
                        i.InvoiceDate,
                        i.DueDate
                    })
                    .ToListAsync(cancellationToken);

                var totalOutstanding = invoices
                    .Where(i => i.PaymentStatus != "PaidInFull")
                    .Sum(i => i.AmountDue);

                var totalInvoices = invoices.Sum(i => i.TotalAmount);
                var completionRate = totalInvoices > 0 ? (totalPaid / totalInvoices) * 100 : 0;

                // حساب متوسط التأخير في الدفع
                var overdueInvoices = invoices
                    .Where(i => i.DueDate.HasValue &&
                                i.DueDate.Value < DateTime.UtcNow &&
                                i.PaymentStatus != "PaidInFull")
                    .ToList();

                var avgDelay = overdueInvoices.Any()
                    ? overdueInvoices.Average(i => (DateTime.UtcNow - i.DueDate!.Value).TotalDays)
                    : 0;

                // Status breakdown
                var statusBreakdown = invoices
                    .GroupBy(i => i.PaymentStatus)
                    .Select(g => new PaymentStatusBreakdownDto
                    {
                        Status = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(i => i.TotalAmount),
                        Percentage = totalInvoices > 0 ? (g.Sum(i => i.TotalAmount) / totalInvoices) * 100 : 0
                    })
                    .ToList();

                // Monthly trends
                var monthlyPayments = payments
                    .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                    .Select(g => new MonthlyPaymentDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy"),
                        TotalPaid = g.Sum(p => p.Amount),
                        TotalDue = 0
                    })
                    .ToList();

                var monthlyInvoices = invoices
                    .GroupBy(i => new { i.InvoiceDate.Year, i.InvoiceDate.Month })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        TotalDue = g.Sum(i => i.AmountDue)
                    })
                    .ToList();

                foreach (var mp in monthlyPayments)
                {
                    var invoice = monthlyInvoices.FirstOrDefault(mi => mi.Year == mp.Year && mi.Month == mp.Month);
                    if (invoice != null)
                    {
                        mp.TotalDue = invoice.TotalDue;
                    }
                }

                var result = new PaymentTrendsDto
                {
                    TotalPaid = totalPaid,
                    TotalOutstanding = totalOutstanding,
                    PaymentCompletionRate = Math.Round(completionRate, 2),
                    AveragePaymentDelay = Math.Round((decimal)avgDelay, 2),
                    StatusBreakdown = statusBreakdown,
                    MonthlyTrends = monthlyPayments.OrderBy(m => m.Year).ThenBy(m => m.Month).ToList()
                };

                _logger.LogInformation("Payment trends retrieved. Paid: {Paid}, Outstanding: {Outstanding}",
                    totalPaid, totalOutstanding);

                return ResponseViewModel<PaymentTrendsDto>.Success(result, "Payment trends retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment trends for CompanyId: {CompanyId}", request.CompanyId);
                throw new BusinessLogicException(
                    "Failed to retrieve payment trends",
                    "Purchases",
                    Common.Enum.Finance_Enum.FinanceErrorCode.DatabaseError);
            }
        }
    }
}

