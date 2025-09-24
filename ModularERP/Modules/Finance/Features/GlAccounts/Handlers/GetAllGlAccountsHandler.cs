using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.GlAccounts.DTO;
using ModularERP.Modules.Finance.Features.GlAccounts.Models;
using ModularERP.Modules.Finance.Features.GlAccounts.Queries;
using ModularERP.Shared.Interfaces;
using Serilog;
using ILogger = Serilog.ILogger;

namespace ModularERP.Modules.Finance.Features.GlAccounts.Handlers
{
    public class GetAllGlAccountsHandler : IRequestHandler<GetAllGlAccountsQuery, ResponseViewModel<List<GlAccountResponseDto>>>
    {
        private readonly IGeneralRepository<GlAccount> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger _logger = Log.ForContext<GetAllGlAccountsHandler>();

        public GetAllGlAccountsHandler(IGeneralRepository<GlAccount> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<List<GlAccountResponseDto>>> Handle(GetAllGlAccountsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.Information("Retrieving GLAccounts with CompanyId: {CompanyId}, SearchTerm: {SearchTerm}, Page: {PageNumber}, PageSize: {PageSize}",
                    request.CompanyId, request.SearchTerm, request.PageNumber, request.PageSize);

                var query = _repository.GetAll().AsQueryable();

                // Filter by CompanyId if provided
                if (request.CompanyId.HasValue)
                {
                    query = _repository.GetByCompanyId(request.CompanyId.Value);
                }

                // Apply search filter
                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    query = query.Where(x => x.Code.Contains(request.SearchTerm) || x.Name.Contains(request.SearchTerm));
                }

                // Apply projection to avoid loading navigation properties
                var glAccountsQuery = query.Select(x => new GlAccount
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Type = x.Type,
                    IsLeaf = x.IsLeaf,
                    CompanyId = x.CompanyId,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    Company = new ModularERP.Modules.Finance.Features.Companys.Models.Company
                    {
                        Name = x.Company.Name
                    }
                });

                // Apply pagination
                var glAccounts = await glAccountsQuery
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);

                var responseDtos = _mapper.Map<List<GlAccountResponseDto>>(glAccounts);

                _logger.Information("Retrieved {Count} GLAccounts successfully", responseDtos.Count);

                return ResponseViewModel<List<GlAccountResponseDto>>.Success(responseDtos, "GLAccounts retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while retrieving GLAccounts");
                throw new BusinessLogicException(
                    "An error occurred while retrieving GLAccounts",
                    "Finance",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
