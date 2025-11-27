using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Models;
using ModularERP.Modules.Finance.Features.Companys.Models;
using ModularERP.Modules.Finance.Features.Currencies.Models;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Modules.Inventory.Features.Services.Models;
using ModularERP.Modules.Purchases.Invoicing.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Suppliers.Models
{
    public class Supplier : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ContactPerson { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        public string? Address { get; set; }

        public SupplierStatus Status { get; set; } = SupplierStatus.Active;

        [Required]
        [MaxLength(50)]
        public string SupplierNumber { get; set; } = string.Empty;

        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public Guid CurrencyId { get; set; }

        [MaxLength(20)]
        public string? Mobile { get; set; }

        [MaxLength(255)]
        public string? StreetAddress1 { get; set; }

        [MaxLength(255)]
        public string? StreetAddress2 { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? State { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        [MaxLength(100)]
        public string? CommercialRegistration { get; set; }

        [MaxLength(50)]
        public string? TaxId { get; set; }

        // Balance Tracking
        public decimal OpeningBalance { get; set; } = 0;
        public DateTime? OpeningBalanceDate { get; set; }
        public decimal CurrentBalance { get; set; } = 0;
        public decimal TotalPurchases { get; set; } = 0;
        public decimal TotalPaid { get; set; } = 0;

        // Navigation Properties
        public virtual Currency Currency { get; set; } = null!;
        public virtual Company? Company { get; set; }
        public virtual ICollection<SupplierContactPerson> ContactPersons { get; set; } = new List<SupplierContactPerson>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Service> Services { get; set; } = new List<Service>();
        public virtual ICollection<SupplierPayment> Payments { get; set; } = new List<SupplierPayment>();
    }
}
