using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POAttachment;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Qeuries.Qeuries_POAttachment
{
    public class GetPOAttachmentsQuery : IRequest<ResponseViewModel<List<POAttachmentResponseDto>>>
    {
        public Guid PurchaseOrderId { get; set; }
    }
}
