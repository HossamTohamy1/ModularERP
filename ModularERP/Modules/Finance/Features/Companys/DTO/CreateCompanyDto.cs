using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Finance.Features.Companys.DTO
{
    public class CreateCompanyDto
    
    {
        [Required(ErrorMessage = "Company name is required")]
        [MaxLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Currency code is required")]
        [MaxLength(3, ErrorMessage = "Currency code must be 3 characters")]
        [MinLength(3, ErrorMessage = "Currency code must be 3 characters")]
        public string CurrencyCode { get; set; } = "EGP";
    }

}
