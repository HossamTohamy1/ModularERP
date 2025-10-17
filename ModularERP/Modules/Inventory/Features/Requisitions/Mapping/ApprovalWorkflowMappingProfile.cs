using AutoMapper;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition_Workflow;
using ModularERP.Modules.Inventory.Features.Requisitions.Models;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Mapping
{
    public class ApprovalWorkflowMappingProfile : Profile
    {
        public ApprovalWorkflowMappingProfile()
        {
            CreateMap<RequisitionApprovalLog, ApprovalLogDto>()
                .ForMember(dest => dest.Action,
                    opt => opt.MapFrom(src => src.Action.ToString()))
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null));

            CreateMap<Requisition, WorkflowStatusDto>()
                .ForMember(dest => dest.RequisitionId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CurrentStatus,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ApprovalLogs,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Draft,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Submitted,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Approved,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Confirmed,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Rejected,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Cancelled,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Reversed,
                    opt => opt.Ignore());
        }
    }
}