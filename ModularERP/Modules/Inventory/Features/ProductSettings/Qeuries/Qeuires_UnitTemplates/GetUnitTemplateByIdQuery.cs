using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_UnitTemplates;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries.Qeuires_UnitTemplates
{
    public record GetUnitTemplateByIdQuery(Guid Id) : IRequest<ResponseViewModel<UnitTemplateDto>>;

}
