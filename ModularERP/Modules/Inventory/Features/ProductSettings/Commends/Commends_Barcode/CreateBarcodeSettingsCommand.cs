using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Barcode;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Barcode
{
    public class CreateBarcodeSettingsCommand : IRequest<ResponseViewModel<BarcodeSettingsDto>>
    {
        public string BarcodeType { get; set; }
        public bool EnableWeightEmbedded { get; set; }
        public string? EmbeddedBarcodeFormat { get; set; }
        public decimal? WeightUnitDivider { get; set; }
        public decimal? CurrencyDivider { get; set; }
        public string? Notes { get; set; }
        public bool IsDefault { get; set; }
    }
}
