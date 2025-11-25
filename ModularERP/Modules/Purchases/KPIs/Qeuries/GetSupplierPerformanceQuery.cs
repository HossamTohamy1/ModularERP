using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.KPIs.DTO;

namespace ModularERP.Modules.Purchases.KPIs.Qeuries
{
    public class GetSupplierPerformanceQuery : IRequest<ResponseViewModel<SupplierPerformanceDto>>
    {
        public Guid CompanyId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TopCount { get; set; } = 10;
    }
}
