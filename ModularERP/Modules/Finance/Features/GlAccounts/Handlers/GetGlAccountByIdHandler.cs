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
    public class GetGlAccountByIdHandler : IRequestHandler<GetGlAccountByIdQuery, ResponseViewModel<GlAccountResponseDto>>
    {
        private readonly IGeneralRepository<GlAccount> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger _logger = Log.ForContext<GetGlAccountByIdHandler>();

        public GetGlAccountByIdHandler(IGeneralRepository<GlAccount> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<GlAccountResponseDto>> Handle(GetGlAccountByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.Information("Retrieving GLAccount with ID: {Id}", request.Id);

                var glAccount = await _repository.GetAll()
                    .Where(x => x.Id == request.Id)
                    .Select(x => new GlAccount
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
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (glAccount == null)
                {
                    _logger.Warning("GLAccount with ID {Id} not found", request.Id);
                    throw new NotFoundException(
                        $"GLAccount with ID '{request.Id}' not found",
                        FinanceErrorCode.NotFound);
                }

                var responseDto = _mapper.Map<GlAccountResponseDto>(glAccount);

                _logger.Information("GLAccount retrieved successfully with ID: {Id}, Code: {Code}",
                    glAccount.Id, glAccount.Code);

                return ResponseViewModel<GlAccountResponseDto>.Success(responseDto, "GLAccount retrieved successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while retrieving GLAccount with ID: {Id}", request.Id);
                throw new BusinessLogicException(
                    "An error occurred while retrieving the GLAccount",
                    "Finance",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
