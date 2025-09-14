﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security.Principal;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using ModularERP.Modules.Finance.Features.GlAccounts.Models;
using ModularERP.Modules.Finance.Features.Currencies.Models;
using ModularERP.Common.Models;

namespace ModularERP.Modules.Finance.Features.LedgerEntries.Models
{
    public class LedgerEntry : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid VoucherId { get; set; }

        public Guid GlAccountId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DebitBase { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditBase { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DebitTxn { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditTxn { get; set; } = 0;

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "EGP";

        [Column(TypeName = "decimal(18,6)")]
        public decimal FxRate { get; set; } = 1.0m;

        public DateTime EntryDate { get; set; } = DateTime.Today;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Voucher Voucher { get; set; } = null!;
        public virtual GlAccount GlAccount { get; set; } = null!;
        public virtual Currency Currency { get; set; } = null!;
    }
}
