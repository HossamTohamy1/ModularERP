namespace ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO
{
    public class AttachmentResponseDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
    }
}
