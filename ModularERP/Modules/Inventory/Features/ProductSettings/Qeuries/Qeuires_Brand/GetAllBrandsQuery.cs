using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Brand;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries.Qeuires_Brand
{
    public record GetAllBrandsQuery : IRequest<ResponseViewModel<List<BrandDto>>>;

}
