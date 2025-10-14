using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;
using ModularERP.Modules.Inventory.Features.Requisitions.Models;
using ModularERP.Modules.Inventory.Features.Requisitions.Qeuries.Qeuier_Requisition;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Handlers.Handlers_Requisition
{
    public class GetPendingApprovalRequisitionsHandler : IRequestHandler<GetPendingApprovalRequisitionsQuery, ResponseViewModel<List<RequisitionListDto>>>
    {
        private readonly FinanceDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetPendingApprovalRequisitionsHandler(FinanceDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<List<RequisitionListDto>>> Handle(
            GetPendingApprovalRequisitionsQuery request,
            CancellationToken cancellationToken)
        {
            var requisitions = await _dbContext.Set<Requisition>()
                .Where(r => r.CompanyId == request.CompanyId && r.Status == RequisitionStatus.Submitted)
                .OrderByDescending(r => r.SubmittedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ProjectTo<RequisitionListDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return ResponseViewModel<List<RequisitionListDto>>.Success(
                requisitions,
                "Pending approval requisitions retrieved successfully."
            );
        }
    }
}
