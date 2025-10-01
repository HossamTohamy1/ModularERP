using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Category
{
    public record DeleteCategoryCommand(Guid Id)
        : IRequest<ResponseViewModel<bool>>;
}
