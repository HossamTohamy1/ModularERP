using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PODeposite;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Qeuries.Qeuries_PODeposite;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Controllers
{
    [Route("api/purchase-orders/{purchaseOrderId}/deposits")]
    [ApiController]
    public class PODepositsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PODepositsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// تسجيل دفعة مقدمة/عربون جديدة لأمر الشراء
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateDeposit(Guid purchaseOrderId, [FromBody] CreatePODepositCommand command)
        {
            command.PurchaseOrderId = purchaseOrderId;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// الحصول على قائمة الدفعات المقدمة لأمر الشراء
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDeposits(Guid purchaseOrderId)
        {
            var query = new GetPODepositsQuery { PurchaseOrderId = purchaseOrderId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// تحديث دفعة مقدمة
        /// </summary>
        [HttpPut("{depositId}")]
        public async Task<IActionResult> UpdateDeposit(Guid purchaseOrderId, Guid depositId, [FromBody] UpdatePODepositCommand command)
        {
            command.Id = depositId;
            command.PurchaseOrderId = purchaseOrderId;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// حذف دفعة مقدمة
        /// </summary>
        [HttpDelete("{depositId}")]
        public async Task<IActionResult> DeleteDeposit(Guid purchaseOrderId, Guid depositId)
        {
            var command = new DeletePODepositCommand
            {
                Id = depositId,
                PurchaseOrderId = purchaseOrderId
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}