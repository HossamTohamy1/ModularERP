using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_POAttachment;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Qeuries.Qeuries_POAttachment;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Controllers
{
    [Route("api/purchase-orders/{purchaseOrderId}/attachments")]
    [ApiController]
    public class POAttachmentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public POAttachmentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// رفع ملف مرفق جديد لأمر الشراء (يدعم Drag & Drop)
        /// </summary>
        [HttpPost]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit - REMOVED DisableRequestSizeLimit conflict
        public async Task<IActionResult> UploadAttachment(Guid purchaseOrderId, IFormFile file)
        {
            var command = new UploadPOAttachmentCommand
            {
                PurchaseOrderId = purchaseOrderId,
                File = file
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// الحصول على قائمة المرفقات لأمر الشراء
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAttachments(Guid purchaseOrderId)
        {
            var query = new GetPOAttachmentsQuery { PurchaseOrderId = purchaseOrderId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// حذف مرفق
        /// </summary>
        [HttpDelete("{attachmentId}")]
        public async Task<IActionResult> DeleteAttachment(Guid purchaseOrderId, Guid attachmentId)
        {
            var command = new DeletePOAttachmentCommand
            {
                Id = attachmentId,
                PurchaseOrderId = purchaseOrderId
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}