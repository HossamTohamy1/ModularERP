using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.DTO
{
    public class TaxProfileComponentRequest
    {
        [Required]
        public Guid TaxComponentId { get; set; }

        [Range(1, 100)]
        public int Priority { get; set; } = 1;
    }
}
