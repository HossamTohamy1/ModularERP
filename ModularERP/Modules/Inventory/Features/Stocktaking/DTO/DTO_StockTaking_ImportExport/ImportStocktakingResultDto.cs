namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_ImportExport
{
    public class ImportStocktakingResultDto
    {
        public Guid StocktakingId { get; set; }
        public int TotalRecords { get; set; }
        public int SuccessfulRecords { get; set; }
        public int FailedRecords { get; set; }
        public List<ImportErrorDto> Errors { get; set; } = new();
        public List<ImportedLineDto> ImportedLines { get; set; } = new();
    }
}
