﻿using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Models;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Finance.Features.RecurringSchedules.Models
{
    public class RecurringSchedule : BaseEntity
    {

        [Required]
        public ScheduleFrequency Frequency { get; set; }

        public string? RRule { get; set; }

        public DateTime NextOccurrence { get; set; }

        public bool IsActive { get; set; } = true;

        public Guid CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ApplicationUser Creator { get; set; } = null!;
        public virtual ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();
    }
}
