using ModularERP.Common.Enum.Finance_Enum;

namespace ModularERP.Modules.Finance.Features.GlAccounts.DTO
{
    public class GlAccountResponseDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public AccountType Type { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public bool IsLeaf { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
