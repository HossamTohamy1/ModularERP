using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Category
{
    public record DeleteCategoryAttachmentCommand(Guid CategoryId, Guid AttachmentId)
        : IRequest<ResponseViewModel<bool>>;
}
