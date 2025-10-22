namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_ImportExport
{
    public class ImportErrorDto
    {
        public int RowNumber { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
