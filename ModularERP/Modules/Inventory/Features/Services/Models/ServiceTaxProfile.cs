using ModularERP.Modules.Inventory.Features.TaxManagement.Models;

namespace ModularERP.Modules.Inventory.Features.Services.Models
{
    public class ServiceTaxProfile
    {
        public Guid ServiceId { get; set; }

        public Guid TaxProfileId { get; set; }

        /// <summary>
        /// Indicates if this is the primary/default tax profile for the service
        /// </summary>
        public bool IsPrimary { get; set; } = false;
        public Guid TenantId { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual Service Service { get; set; } = null!;
        public virtual TaxProfile TaxProfile { get; set; } = null!;
    }
}
