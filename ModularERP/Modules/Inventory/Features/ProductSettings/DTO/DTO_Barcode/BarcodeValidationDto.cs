namespace ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Barcode
{
    public class BarcodeValidationDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
