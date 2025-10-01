using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Brand;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Brand
{
    public record UpdateBrandCommand(UpdateBrandDto Dto) : IRequest<ResponseViewModel<BrandDto>>;

}
