using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Companys.DTO;
using ModularERP.Modules.Finance.Features.Companys.Models;
using ModularERP.Modules.Finance.Features.Companys.Queries;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.Companys.Handlers
{
    public class GetCompanyByIdHandler : IRequestHandler<GetCompanyByIdQuery, ResponseViewModel<CompanyResponseDto>>
    {
        private readonly IGeneralRepository<Company> _companyRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCompanyByIdHandler> _logger;

        public GetCompanyByIdHandler(
            IGeneralRepository<Company> companyRepository,
            IMapper mapper,
            ILogger<GetCompanyByIdHandler> logger)
        {
            _companyRepository = companyRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<CompanyResponseDto>> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving company with ID: {CompanyId}", request.Id);

            try
            {
                var company = await _companyRepository.GetByID(request.Id);
                if (company == null)
                {
                    _logger.LogWarning("Company with ID {CompanyId} not found", request.Id);
                    throw new NotFoundException($"Company with ID '{request.Id}' not found", FinanceErrorCode.NotFound);
                }

                var responseDto = _mapper.Map<CompanyResponseDto>(company);

                _logger.LogInformation("Successfully retrieved company with ID: {CompanyId}", request.Id);

                return ResponseViewModel<CompanyResponseDto>.Success(
                    responseDto,
                    "Company retrieved successfully"
                );
            }
            catch (Exception ex) when (!(ex is BaseApplicationException))
            {
                _logger.LogError(ex, "Unexpected error occurred while retrieving company with ID: {CompanyId}", request.Id);
                throw new BusinessLogicException("An error occurred while retrieving the company", "Finance", FinanceErrorCode.InternalServerError);
            }
        }
    }
}
