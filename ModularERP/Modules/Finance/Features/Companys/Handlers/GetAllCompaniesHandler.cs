using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Companys.DTO;
using ModularERP.Modules.Finance.Features.Companys.Models;
using ModularERP.Modules.Finance.Features.Companys.Queries;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.Companys.Handlers
{
    public class GetAllCompaniesHandler : IRequestHandler<GetAllCompaniesQuery, ResponseViewModel<List<CompanyResponseDto>>>
    {
        private readonly IGeneralRepository<Company> _companyRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllCompaniesHandler> _logger;

        public GetAllCompaniesHandler(
            IGeneralRepository<Company> companyRepository,
            IMapper mapper,
            ILogger<GetAllCompaniesHandler> logger)
        {
            _companyRepository = companyRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<CompanyResponseDto>>> Handle(GetAllCompaniesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving all companies");

            try
            {
                var companies = await _companyRepository.GetAll()
                    .OrderBy(c => c.Name)
                    .ToListAsync(cancellationToken);

                var responseDtos = _mapper.Map<List<CompanyResponseDto>>(companies);

                _logger.LogInformation("Successfully retrieved {CompanyCount} companies", companies.Count);

                return ResponseViewModel<List<CompanyResponseDto>>.Success(
                    responseDtos,
                    $"Retrieved {companies.Count} companies successfully"
                );
            }
            catch (Exception ex) when (!(ex is BaseApplicationException))
            {
                _logger.LogError(ex, "Unexpected error occurred while retrieving companies");
                throw new BusinessLogicException("An error occurred while retrieving companies", "Finance", FinanceErrorCode.InternalServerError);
            }
        }
    }
}
