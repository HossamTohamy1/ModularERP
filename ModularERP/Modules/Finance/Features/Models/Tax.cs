using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ModularERP.Common.Models;
using ModularERP.Common.Enum.Finance_Enum;

namespace ModularERP.Modules.Finance.Features.Models
{
    public class Tax : BaseEntity
    {

        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,2)")]
        public decimal Rate { get; set; }

        public TaxType Type { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<VoucherTax> VoucherTaxes { get; set; } = new List<VoucherTax>();
    }
}
