﻿using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Models;
using ModularERP.Modules.Finance.Features.Companys.Models;
using ModularERP.Modules.Finance.Features.LedgerEntries.Models;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Finance.Features.GlAccounts.Models
{
    public class GlAccount : BaseEntity
    {

        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public AccountType Type { get; set; }

        public bool IsLeaf { get; set; } = true;

        public Guid CompanyId { get; set; }

        // Navigation properties
        public virtual Company Company { get; set; } = null!;
        public virtual ICollection<Voucher> CategoryVouchers { get; set; } = new List<Voucher>();
        public virtual ICollection<Voucher> JournalVouchers { get; set; } = new List<Voucher>();
        public virtual ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
    }
}
