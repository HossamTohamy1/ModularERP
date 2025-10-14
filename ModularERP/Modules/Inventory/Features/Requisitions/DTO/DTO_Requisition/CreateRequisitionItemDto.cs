using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition
{
    public class CreateRequisitionItemDto
    {
        [Required(ErrorMessage = "Product is required")]
        public Guid ProductId { get; set; }

        public decimal? UnitPrice { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

 
    }
}
