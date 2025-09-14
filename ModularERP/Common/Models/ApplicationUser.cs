using Microsoft.AspNetCore.Identity;
using ModularERP.Modules.Finance.Features.Attachments.Models;
using ModularERP.Modules.Finance.Features.AuditLogs.Models;
using ModularERP.Modules.Finance.Features.RecurringSchedules.Models;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using System.Net.Mail;

namespace ModularERP.Common.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Voucher> CreatedVouchers { get; set; } = new List<Voucher>();
        public virtual ICollection<Voucher> PostedVouchers { get; set; } = new List<Voucher>();
        public virtual ICollection<Voucher> ReversedVouchers { get; set; } = new List<Voucher>();
        public virtual ICollection<VoucherAttachment> UploadedAttachments { get; set; } = new List<VoucherAttachment>();
        public virtual ICollection<RecurringSchedule> CreatedSchedules { get; set; } = new List<RecurringSchedule>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
