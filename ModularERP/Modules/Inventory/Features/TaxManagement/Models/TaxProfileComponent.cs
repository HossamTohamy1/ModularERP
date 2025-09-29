using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Models
{
    public class TaxProfileComponent
    {
        public Guid TaxProfileId { get; set; }

        public Guid TaxComponentId { get; set; }


        [Range(1, 100)]
        public int Priority { get; set; } = 1;

        // Navigation Properties
        public virtual TaxProfile TaxProfile { get; set; } = null!;
        public virtual TaxComponent TaxComponent { get; set; } = null!;
    }
}
