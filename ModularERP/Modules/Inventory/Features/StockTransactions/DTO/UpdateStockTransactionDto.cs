using ModularERP.Common.Enum.Inventory_Enum;

namespace ModularERP.Modules.Inventory.Features.StockTransactions.DTO
{
    public class UpdateStockTransactionDto
    {
        public Guid Id { get; set; }
        public StockTransactionType TransactionType { get; set; }
        public decimal Quantity { get; set; }
        public decimal? UnitCost { get; set; }
        public string? ReferenceType { get; set; }
        public Guid? ReferenceId { get; set; }
    }
}
