﻿using ModularERP.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Finance.Features.Models
{
    public class Customer : BaseEntity
    {

        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? TaxId { get; set; }

        public bool IsActive { get; set; } = true;

    }
}
