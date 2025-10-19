namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_WorkFlow
{
    public class AdjustmentPreviewDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal PhysicalCount { get; set; }
        public decimal Variance { get; set; }
        public string AdjustmentType { get; set; }
        public decimal UnitCost { get; set; }
        public decimal AdjustmentValue { get; set; }
    }
}