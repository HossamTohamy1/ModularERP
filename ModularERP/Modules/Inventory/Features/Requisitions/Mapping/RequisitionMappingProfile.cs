using AutoMapper;
using ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_Requisition;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;
using ModularERP.Modules.Inventory.Features.Requisitions.Models;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Mapping
{
    public class RequisitionMappingProfile : Profile
    {
        public RequisitionMappingProfile()
        {
            // Command to Entity
            CreateMap<CreateRequisitionCommand, Requisition>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Number, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Items, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore());

            CreateMap<UpdateRequisitionCommand, Requisition>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Number, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Items, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore());

            CreateMap<CreateRequisitionItemDto, RequisitionItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.RequisitionId, opt => opt.Ignore())
                .ForMember(dest => dest.StockOnHand, opt => opt.Ignore())
                .ForMember(dest => dest.NewStockOnHand, opt => opt.Ignore())
                .ForMember(dest => dest.LineTotal, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore());

            CreateMap<RequisitionAttachmentDto, RequisitionAttachment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.RequisitionId, opt => opt.Ignore())
                .ForMember(dest => dest.UploadedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UploadedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore());

            // Entity to DTO - Using Projection with explicit navigation
            CreateMap<Requisition, RequisitionResponseDto>()
                   .ForMember(dest => dest.WarehouseName,
                       opt => opt.MapFrom(src => src.Warehouse.Name ?? string.Empty))
                   .ForMember(dest => dest.SupplierName,
                       opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : null))
                   .ForMember(dest => dest.CreatedBy,
                       opt => opt.MapFrom(src => src.CreatedById ?? Guid.Empty))
                   .ForMember(dest => dest.CreatedByName,
                       opt => opt.MapFrom(src => src.CreatedByUser != null
                           ? (src.CreatedByUser.UserName ?? string.Empty)
                           : string.Empty))
                   .ForMember(dest => dest.ItemsTotal,
                       opt => opt.MapFrom(src => src.Items.Sum(i => i.LineTotal ?? 0)))
                   .ForMember(dest => dest.ItemsCount,
                       opt => opt.MapFrom(src => src.Items.Count))
                   .ForMember(dest => dest.Items,
                       opt => opt.MapFrom(src => src.Items))
                   .ForMember(dest => dest.Attachments,
                       opt => opt.MapFrom(src => src.Attachments));

            CreateMap<Requisition, RequisitionListDto>()
                .ForMember(dest => dest.WarehouseName,
                    opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
                .ForMember(dest => dest.SupplierName,
                    opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : null))
                .ForMember(dest => dest.CreatedByName,
                    opt => opt.MapFrom(src => src.CreatedByUser != null ? src.CreatedByUser.UserName ?? string.Empty : string.Empty))
                .ForMember(dest => dest.ItemsTotal,
                    opt => opt.MapFrom(src => src.Items.Sum(i => i.LineTotal ?? 0)))
                .ForMember(dest => dest.ItemsCount,
                    opt => opt.MapFrom(src => src.Items.Count));

            CreateMap<RequisitionItem, RequisitionItemDto>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.ProductSKU,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty));

            CreateMap<RequisitionAttachment, RequisitionAttachmentResponseDto>()
                .ForMember(dest => dest.UploadedByName,
                    opt => opt.MapFrom(src => src.UploadedByUser != null ? src.UploadedByUser.UserName ?? string.Empty : string.Empty));
        }
    }
}