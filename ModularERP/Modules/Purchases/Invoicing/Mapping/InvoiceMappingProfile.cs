using AutoMapper;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_InvocieItem;
using ModularERP.Modules.Purchases.Invoicing.Models;

namespace ModularERP.Modules.Purchases.Invoicing.Mapping
{
    public class InvoiceMappingProfile : Profile
    {
        public InvoiceMappingProfile()
        {
            // PurchaseInvoice -> InvoiceResponse
            CreateMap<PurchaseInvoice, InvoiceResponse>()
                .ForMember(dest => dest.PurchaseOrderNumber, opt => opt.MapFrom(src => src.PurchaseOrder.PONumber))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.Name))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier.Name));

            // InvoiceLineItem -> InvoiceLineItemResponse
            CreateMap<InvoiceLineItem, InvoiceLineItemResponse>();

            // AddInvoiceItemRequest -> InvoiceLineItem
            CreateMap<AddInvoiceItemRequest, InvoiceLineItem>()
                .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => (src.Quantity * src.UnitPrice) + src.TaxAmount))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore());

            // UpdateInvoiceItemRequest -> InvoiceLineItem
            CreateMap<UpdateInvoiceItemRequest, InvoiceLineItem>()
                .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => (src.Quantity * src.UnitPrice) + src.TaxAmount))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.POLineItemId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore());
        }
    }
}