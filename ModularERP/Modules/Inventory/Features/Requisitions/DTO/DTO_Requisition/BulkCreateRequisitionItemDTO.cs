using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition
{
    public class BulkCreateRequisitionItemDTO
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<CreateRequisitionItemDto> Items { get; set; } = new();
    }
}
