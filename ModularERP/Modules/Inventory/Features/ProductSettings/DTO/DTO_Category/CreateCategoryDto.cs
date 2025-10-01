namespace ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Category
{
    public class CreateCategoryDto : CategoryBaseDto
    {
        public List<IFormFile>? Attachments { get; set; }
    }
}
