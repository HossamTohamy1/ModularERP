using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Commends
{
    public record DeleteCategoryAttachmentCommand(Guid CategoryId, Guid AttachmentId)
        : IRequest<ResponseViewModel<bool>>;
}
