namespace ModularERP.Modules.Inventory.Features.ProductSettings.DTO
{
    public class CreateCategoryDto : CategoryBaseDto
    {
        public List<IFormFile>? Attachments { get; set; }
    }
}
