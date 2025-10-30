using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POAttachment;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_POAttachment
{
    public class UploadPOAttachmentCommand : IRequest<ResponseViewModel<POAttachmentResponseDto>>
    {
        public Guid PurchaseOrderId { get; set; }
        public IFormFile File { get; set; } = null!;
    }

}
