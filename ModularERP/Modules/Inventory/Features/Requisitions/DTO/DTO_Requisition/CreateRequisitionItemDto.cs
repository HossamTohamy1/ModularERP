namespace ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition
{
    public class CreateRequisitionItemDto
    {
        public Guid ProductId { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal Quantity { get; set; }
    }
}
