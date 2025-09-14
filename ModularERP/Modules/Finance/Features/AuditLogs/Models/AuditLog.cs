using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using ModularERP.Common.Models;

namespace ModularERP.Modules.Finance.Features.AuditLogs.Models
{
    public class AuditLog : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid VoucherId { get; set; }

        [Required, MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }

        public Guid UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Voucher Voucher { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
