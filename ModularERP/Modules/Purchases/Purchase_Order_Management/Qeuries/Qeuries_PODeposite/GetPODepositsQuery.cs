using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PODeposite;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Qeuries.Qeuries_PODeposite
{
    public class GetPODepositsQuery : IRequest<ResponseViewModel<List<PODepositResponseDto>>>
    {
        public Guid PurchaseOrderId { get; set; }
    }
}