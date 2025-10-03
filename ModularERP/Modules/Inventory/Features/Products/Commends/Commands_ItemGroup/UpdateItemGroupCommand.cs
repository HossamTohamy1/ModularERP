using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ItemGroup;

namespace ModularERP.Modules.Inventory.Features.Products.Commends.Commands_ItemGroup
{
    public class UpdateItemGroupCommand : IRequest<ResponseViewModel<ItemGroupDto>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? CategoryId { get; set; }
        public Guid? BrandId { get; set; }
        public string? Description { get; set; }
    }
}
