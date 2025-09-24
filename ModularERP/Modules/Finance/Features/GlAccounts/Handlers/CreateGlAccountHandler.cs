using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.GlAccounts.Commands;
using ModularERP.Modules.Finance.Features.GlAccounts.DTO;
using ModularERP.Modules.Finance.Features.GlAccounts.Models;
using ModularERP.Shared.Interfaces;
using Serilog;
using ILogger = Serilog.ILogger;

namespace ModularERP.Modules.Finance.Features.GlAccounts.Handlers
{
    public class CreateGlAccountHandler : IRequestHandler<CreateGlAccountCommand, ResponseViewModel<GlAccountResponseDto>>
    {
        private readonly IGeneralRepository<GlAccount> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger _logger = Log.ForContext<CreateGlAccountHandler>();

        public CreateGlAccountHandler(IGeneralRepository<GlAccount> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<GlAccountResponseDto>> Handle(CreateGlAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.Information("Starting GLAccount creation process for Code: {Code}, Company: {CompanyId}",
                    request.GlAccount.Code, request.GlAccount.CompanyId);

                // Check if code already exists
                var existingAccount = await _repository.AnyAsync(x => x.Code == request.GlAccount.Code && x.CompanyId == request.GlAccount.CompanyId);
                if (existingAccount)
                {
                    _logger.Warning("GLAccount creation failed - Code {Code} already exists in Company {CompanyId}",
                        request.GlAccount.Code, request.GlAccount.CompanyId);
                    throw new BusinessLogicException(
                        $"GLAccount with code '{request.GlAccount.Code}' already exists in this company",
                        "Finance",
                        FinanceErrorCode.DuplicateRecord);
                }

                // Map and create entity
                var glAccount = _mapper.Map<GlAccount>(request.GlAccount);

                await _repository.AddAsync(glAccount);
                await _repository.SaveChanges();

                _logger.Information("GLAccount created successfully with ID: {Id}, Code: {Code}",
                    glAccount.Id, glAccount.Code);

                // Get created entity with company info
                var createdAccount = await _repository.GetByCompanyId(request.GlAccount.CompanyId)
                    .Where(x => x.Id == glAccount.Id)
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
                        Company = new Companys.Models.Company
                        {
                            Name = x.Company.Name
                        }
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                var responseDto = _mapper.Map<GlAccountResponseDto>(createdAccount);

                return ResponseViewModel<GlAccountResponseDto>.Success(responseDto, "GLAccount created successfully");
            }
            catch (BusinessLogicException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while creating GLAccount for Code: {Code}", request.GlAccount.Code);
                throw new BusinessLogicException(
                    "An error occurred while creating the GLAccount",
                    "Finance",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
