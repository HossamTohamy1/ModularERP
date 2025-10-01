using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Brand
{
    public class CreateBrandDto
    {
        [Required(ErrorMessage = "Brand name is required")]
        [MaxLength(100, ErrorMessage = "Brand name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [MaxLength(500, ErrorMessage = "Logo path cannot exceed 500 characters")]
        public string? LogoPath { get; set; }
    }
}
