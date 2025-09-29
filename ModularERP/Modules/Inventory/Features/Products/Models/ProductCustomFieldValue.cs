using ModularERP.Common.Models;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;

namespace ModularERP.Modules.Inventory.Features.Products.Models
{
    public class ProductCustomFieldValue : BaseEntity
    {
        public Guid ProductId { get; set; }

        public Guid FieldId { get; set; }

        public string? FieldValue { get; set; }

        // Navigation Properties
        public virtual Product Product { get; set; } = null!;
        public virtual CustomField CustomField { get; set; } = null!;
    }
}
