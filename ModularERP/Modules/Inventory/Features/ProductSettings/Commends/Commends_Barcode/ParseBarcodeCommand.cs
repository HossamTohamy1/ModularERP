using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Barcode;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Barcode
{
    public class ParseBarcodeCommand : IRequest<ResponseViewModel<ParsedBarcodeDto>>
    {
        public string Barcode { get; set; }
    }
}
