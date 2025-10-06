using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commands_UnitTemplates
{
    public record DeleteUnitConversionCommand(Guid UnitTemplateId, Guid UnitConversionId) : IRequest<ResponseViewModel<bool>>;


}
