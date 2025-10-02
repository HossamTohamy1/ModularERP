using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Commends
{
    public class DeleteTaxProfileCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }
    }
}
