using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Commends
{
    public class RemoveTaxComponentFromProfileCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid TaxProfileId { get; set; }
        public Guid TaxComponentId { get; set; }
    }
}
