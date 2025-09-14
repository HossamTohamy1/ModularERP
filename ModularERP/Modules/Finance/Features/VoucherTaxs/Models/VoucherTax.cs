using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Models;
using ModularERP.Modules.Finance.Features.Taxs.Models;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModularERP.Modules.Finance.Features.VoucherTaxs.Models
{
    public class VoucherTax : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid VoucherId { get; set; }

        public Guid TaxId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        public bool IsWithholding { get; set; } = false;

        public TaxDirection Direction { get; set; }

        // Navigation properties
        public virtual Voucher Voucher { get; set; } = null!;
        public virtual Tax Tax { get; set; } = null!;
    }
}
