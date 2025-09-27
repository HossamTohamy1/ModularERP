namespace ModularERP.Modules.Finance.Features.Treasuries.DTO
{
    public class TreasuryCreatedDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? JournalAccountId { get; set; }  // FK to GLAccount

        public DateTime CreatedAt { get; set; }
    }
}
