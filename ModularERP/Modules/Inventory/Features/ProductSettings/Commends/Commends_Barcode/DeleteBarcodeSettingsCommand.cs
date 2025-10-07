using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Barcode
{
    public class DeleteBarcodeSettingsCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }
    }
}
