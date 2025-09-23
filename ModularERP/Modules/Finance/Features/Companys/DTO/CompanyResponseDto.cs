namespace ModularERP.Modules.Finance.Features.Companys.DTO
{
    public class CompanyResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
