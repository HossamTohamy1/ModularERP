using AutoMapper;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Barcode;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Barcode;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Mapping
{
    public class BarcodeSettingsMappingProfile : Profile
    {
        public BarcodeSettingsMappingProfile()
        {
            // Entity to DTO
            CreateMap<BarcodeSettings, BarcodeSettingsDto>();

            // Command to Entity
            CreateMap<CreateBarcodeSettingsCommand, BarcodeSettings>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

            CreateMap<UpdateBarcodeSettingsCommand, BarcodeSettings>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // DTO to Command
            CreateMap<CreateBarcodeSettingsDto, CreateBarcodeSettingsCommand>();
            CreateMap<UpdateBarcodeSettingsDto, UpdateBarcodeSettingsCommand>();
        }
    }
}