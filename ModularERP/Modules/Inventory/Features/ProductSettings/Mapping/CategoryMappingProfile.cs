using AutoMapper;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Category;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Mapping
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            // Category Mappings
            CreateMap<CreateCategoryDto, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.SubCategories, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore())
                .ForMember(dest => dest.ParentCategory, opt => opt.Ignore());

            CreateMap<UpdateCategoryDto, Category>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.SubCategories, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore())
                .ForMember(dest => dest.ParentCategory, opt => opt.Ignore());

            CreateMap<Category, CategoryResponseDto>()
                .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src =>
                    src.ParentCategory != null ? src.ParentCategory.Name : null))
                .ForMember(dest => dest.SubCategoriesCount, opt => opt.MapFrom(src => src.SubCategories.Count))
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments));

            CreateMap<Category, CategoryListDto>()
                .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src =>
                    src.ParentCategory != null ? src.ParentCategory.Name : null))
                .ForMember(dest => dest.SubCategoriesCount, opt => opt.MapFrom(src => src.SubCategories.Count))
                .ForMember(dest => dest.AttachmentsCount, opt => opt.MapFrom(src => src.Attachments.Count));

            // Category Attachment Mappings
            CreateMap<CategoryAttachment, CategoryAttachmentDto>()
                .ForMember(dest => dest.FileSizeFormatted, opt => opt.MapFrom(src => FormatFileSize(src.FileSize)))
                .ForMember(dest => dest.UploadedByName, opt => opt.MapFrom(src =>
                    src.UploadedByUser != null
                        ? $"{src.UploadedByUser.FirstName} {src.UploadedByUser.LastName}".Trim()
                        : null));
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}