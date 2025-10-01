using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Category
{
    public class UpdateCategoryDto : CategoryBaseDto
    {
        [Required]
        public Guid Id { get; set; }

        public List<IFormFile>? NewAttachments { get; set; }
        public List<Guid>? AttachmentIdsToDelete { get; set; }
    }
}
