namespace ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceCalculation
{
    public class PreviewPriceRequestDTO
    {
        public Guid ProductId { get; set; }
        public Guid? PriceListId { get; set; }
        public Guid? CustomerId { get; set; }
        public int Quantity { get; set; }
        public string TransactionType { get; set; } = "Sale";
    }
}
