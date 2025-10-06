using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commands_UnitTemplates
{
    public record DeleteUnitTemplateCommand(Guid Id) : IRequest<ResponseViewModel<bool>>;

}
