using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;
using ModularERP.Modules.Inventory.Features.Requisitions.Models;
using ModularERP.Modules.Inventory.Features.Requisitions.Qeuries.Qeuier_Requisition;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Handlers.Handlers_Requisition
{
    public class GetRequisitionsHandler : IRequestHandler<GetRequisitionsQuery, ResponseViewModel<PaginatedResponseViewModel<RequisitionListDto>>>
    {
        private readonly FinanceDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetRequisitionsHandler(FinanceDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<PaginatedResponseViewModel<RequisitionListDto>>> Handle(
            GetRequisitionsQuery request,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<Requisition>()
                .Where(r => r.CompanyId == request.CompanyId);

            // Apply filters
            if (request.WarehouseId.HasValue)
            {
                query = query.Where(r => r.WarehouseId == request.WarehouseId.Value);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(r => r.Status == request.Status.Value);
            }

            if (request.Type.HasValue)
            {
                query = query.Where(r => r.Type == request.Type.Value);
            }

            if (request.DateFrom.HasValue)
            {
                query = query.Where(r => r.Date >= request.DateFrom.Value);
            }

            if (request.DateTo.HasValue)
            {
                query = query.Where(r => r.Date <= request.DateTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(r =>
                    r.Number.ToLower().Contains(searchLower) ||
                    (r.Notes != null && r.Notes.ToLower().Contains(searchLower)));
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination and projection
            var requisitions = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ProjectTo<RequisitionListDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            var paginatedResult = new PaginatedResponseViewModel<RequisitionListDto>
            {
                Data = requisitions,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };

            return ResponseViewModel<PaginatedResponseViewModel<RequisitionListDto>>.Success(
                paginatedResult,
                "Requisitions retrieved successfully."
            );
        }
    }
}