using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.KPIs.DTO;

namespace ModularERP.Modules.Purchases.KPIs.Qeuries
{
    public class GetPurchaseVolumeQuery : IRequest<ResponseViewModel<PurchaseVolumeDto>>
    {
        public Guid CompanyId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
