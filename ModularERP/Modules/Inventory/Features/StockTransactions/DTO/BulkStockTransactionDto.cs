namespace ModularERP.Modules.Inventory.Features.StockTransactions.DTO
{
    public class BulkStockTransactionDto
    {
        public List<CreateStockTransactionDto> Transactions { get; set; } = new();

    }
}
