namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO
{
    public class BulkStockTransactionDto
    {
        public List<CreateStockTransactionDto> Transactions { get; set; } = new();

    }
}
