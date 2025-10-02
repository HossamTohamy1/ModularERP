using ModularERP.Modules.Inventory.Features.TaxManagement.Models;

namespace ModularERP.Modules.Inventory.Features.Products.Models
{
    public class ProductTaxProfile
    {
        public Guid ProductId { get; set; }

        public Guid TaxProfileId { get; set; }

        /// <summary>
        /// Indicates if this is the primary/default tax profile for the product
        /// </summary>
        public bool IsPrimary { get; set; } = false;
        public Guid TenantId { get; set; }
        public bool IsDeleted { get; set; }
        // Navigation Properties
        public virtual Product Product { get; set; } = null!;
        public virtual TaxProfile TaxProfile { get; set; } = null!;
    }
}
