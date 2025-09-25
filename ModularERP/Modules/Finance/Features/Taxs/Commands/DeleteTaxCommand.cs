using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Finance.Features.Taxs.Commands
{
    public class DeleteTaxCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid TaxId { get; set; }

        public DeleteTaxCommand(Guid taxId)
        {
            TaxId = taxId;
        }
    }
}
