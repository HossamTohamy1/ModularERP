using System.Text.Json.Serialization;

namespace ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition
{
    public class RequisitionItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid RequisitionId { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public decimal? UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? StockOnHand { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? NewStockOnHand { get; set; }
        public decimal? LineTotal { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
