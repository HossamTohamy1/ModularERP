using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Barcode;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries.Qeuires_Barcode
{
    public class GetBarcodeSettingsQuery : IRequest<ResponseViewModel<BarcodeSettingsDto>>
    {
    }
}
