namespace ModularERP.Modules.Finance.Features.BankAccounts.DTO
{
    public class BankAccountCreatedDto
    {
        public Guid Id { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
