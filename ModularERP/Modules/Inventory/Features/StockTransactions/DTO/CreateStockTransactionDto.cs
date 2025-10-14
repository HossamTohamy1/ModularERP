using ModularERP.Common.Enum.Inventory_Enum;

namespace ModularERP.Modules.Inventory.Features.StockTransactions.DTO
{
    public class CreateStockTransactionDto
    {
        public Guid CompanyId { get; set; }
        public Guid ProductId { get; set; }
        public Guid WarehouseId { get; set; }
        public StockTransactionType TransactionType { get; set; }
        public decimal Quantity { get; set; }
        public decimal? UnitCost { get; set; }
        public string? ReferenceType { get; set; }
        public Guid? ReferenceId { get; set; }
    }
}
