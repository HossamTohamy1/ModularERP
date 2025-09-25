using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Finance.Features.Taxs.Commands
{
    public class ToggleTaxStatusCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid TaxId { get; set; }

        public ToggleTaxStatusCommand(Guid taxId)
        {
            TaxId = taxId;
        }
    }
}
