using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;
using ModularERP.Modules.Inventory.Features.Requisitions.Models;
using ModularERP.Modules.Inventory.Features.Requisitions.Qeuries.Qeuier_Requisition;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Handlers.Handlers_Requisition
{
    public class GetRequisitionByIdHandler : IRequestHandler<GetRequisitionByIdQuery, ResponseViewModel<RequisitionResponseDto>>
    {
        private readonly FinanceDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetRequisitionByIdHandler(FinanceDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<RequisitionResponseDto>> Handle(
            GetRequisitionByIdQuery request,
            CancellationToken cancellationToken)
        {
            var requisition = await _dbContext.Set<Requisition>()
                .Where(r => r.Id == request.Id && r.CompanyId == request.CompanyId)
                .ProjectTo<RequisitionResponseDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (requisition == null)
            {
                throw new NotFoundException(
                    $"Requisition with ID {request.Id} not found.",
                    FinanceErrorCode.NotFound
                );
            }

            return ResponseViewModel<RequisitionResponseDto>.Success(
                requisition,
                "Requisition retrieved successfully."
            );
        }
    }
}