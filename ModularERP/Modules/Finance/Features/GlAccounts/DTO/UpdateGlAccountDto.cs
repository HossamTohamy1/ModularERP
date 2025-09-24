using ModularERP.Common.Enum.Finance_Enum;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Finance.Features.GlAccounts.DTO
{
    public class UpdateGlAccountDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public AccountType Type { get; set; }

        public bool IsLeaf { get; set; } = true;

        [Required]
        public Guid CompanyId { get; set; }
    }
}
