using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Brand
{
    public record DeleteBrandCommand(Guid Id) : IRequest<ResponseViewModel<bool>>;

}
