using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.TaxManagement.DTO;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Qeuries
{
    public class GetAllTaxComponentsQuery : IRequest<ResponseViewModel<List<TaxComponentDto>>>
    {
    }
}
