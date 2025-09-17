using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Finance.Features.IncomesVoucher.DTO
{
    public class CounterpartyDto
    {
        [Required]
        public string Type { get; set; } = string.Empty; // Vendor, Customer, Other
        public Guid? Id { get; set; }
    }

}
