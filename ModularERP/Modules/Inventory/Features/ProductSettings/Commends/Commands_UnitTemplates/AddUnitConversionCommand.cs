using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_UnitTemplates;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commands_UnitTemplates
{
    public record AddUnitConversionCommand(Guid UnitTemplateId, CreateUnitConversionDto Data) : IRequest<ResponseViewModel<UnitConversionDto>>;

}
