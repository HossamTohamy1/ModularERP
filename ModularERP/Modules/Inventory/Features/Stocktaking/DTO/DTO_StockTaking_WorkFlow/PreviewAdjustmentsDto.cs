namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_WorkFlow
{
    public class PreviewAdjustmentsDto
    {
        public Guid StocktakingId { get; set; }
        public bool IsRecordOnly { get; set; }
        public int TotalAdjustments { get; set; }
        public int TotalWriteOffs { get; set; }
        public int TotalWriteOns { get; set; }
        public decimal TotalValue { get; set; }
        public List<AdjustmentPreviewDto> Adjustments { get; set; } = new List<AdjustmentPreviewDto>();
    }

}
