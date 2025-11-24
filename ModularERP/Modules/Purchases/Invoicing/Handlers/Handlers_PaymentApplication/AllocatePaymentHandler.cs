using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.Commends.Commands_PaymentApplication;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_PaymentApplication;

namespace ModularERP.Modules.Purchases.Invoicing.Handlers.Handlers_PaymentApplication
{
    public class AllocatePaymentHandler : IRequestHandler<AllocatePaymentCommand, ResponseViewModel<PaymentApplicationSummaryDto>>
    {
        private readonly IMediator _mediator;

        public AllocatePaymentHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ResponseViewModel<PaymentApplicationSummaryDto>> Handle(
            AllocatePaymentCommand request,
            CancellationToken cancellationToken)
        {
            var applyDto = new ApplyPaymentDto
            {
                PaymentId = request.PaymentId,
                Allocations = request.Allocations
            };

            var command = new ApplyPaymentCommand(request.PaymentId, applyDto);
            return await _mediator.Send(command, cancellationToken);
        }
    }
}
