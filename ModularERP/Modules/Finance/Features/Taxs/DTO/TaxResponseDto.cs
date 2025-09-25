using ModularERP.Common.Enum.Finance_Enum;

namespace ModularERP.Modules.Finance.Features.Taxs.DTO
{
    public class TaxResponseDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public TaxType Type { get; set; }
        public string TypeName => Type.ToString();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int VoucherTaxesCount { get; set; }
    }
}
