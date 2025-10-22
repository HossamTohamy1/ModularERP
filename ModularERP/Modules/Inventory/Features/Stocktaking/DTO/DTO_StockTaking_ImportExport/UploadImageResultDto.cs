namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_ImportExport
{
    public class UploadImageResultDto
    {
        public Guid LineId { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }
}
