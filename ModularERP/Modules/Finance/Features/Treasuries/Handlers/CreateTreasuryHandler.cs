using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Treasuries.Commands;
using ModularERP.Modules.Finance.Features.Treasuries.DTO;
using ModularERP.Modules.Finance.Features.Treasuries.Models;
using ModularERP.SharedKernel.Interfaces;

namespace ModularERP.Modules.Finance.Features.Treasuries.Handlers
{
    public class CreateTreasuryHandler : IRequestHandler<CreateTreasuryCommand, ResponseViewModel<TreasuryCreatedDto>>
    {
        private readonly IGeneralRepository<Treasury> _treasuryRepository;
        private readonly IMapper _mapper;

        public CreateTreasuryHandler(IGeneralRepository<Treasury> treasuryRepository, IMapper mapper)
        {
            _treasuryRepository = treasuryRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<TreasuryCreatedDto>> Handle(CreateTreasuryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingTreasury = await _treasuryRepository
                    .Get(t => t.CompanyId == request.Treasury.CompanyId && t.Name == request.Treasury.Name)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingTreasury != null)
                {
                    return ResponseViewModel<TreasuryCreatedDto>.Error(
                        "Treasury with this name already exists for the company",
                        FinanceErrorCode.TreasuryAlreadyExists);
                }

                var treasury = _mapper.Map<Treasury>(request.Treasury);
                treasury.Id = Guid.NewGuid();
                treasury.CreatedAt = DateTime.UtcNow;

                await _treasuryRepository.AddAsync(treasury);
                await _treasuryRepository.SaveChanges();

                var result = _mapper.Map<TreasuryCreatedDto>(treasury);
                return ResponseViewModel<TreasuryCreatedDto>.Success(result, "Treasury created successfully");
            }
            catch (Exception ex)
            {
                return ResponseViewModel<TreasuryCreatedDto>.Error(
                    "An error occurred while creating the treasury",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
