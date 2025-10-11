using ModularERP.Common.Models;
using ModularERP.Modules.Finance.Features.Companys.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Finance.Features.Vendor.Models
{
    public class Vendor : BaseEntity
    {

        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? TaxId { get; set; }

        public bool IsActive { get; set; } = true;
        public Guid CompanyId { get; set; }
        public Company Company { get; set; }
    }
}
