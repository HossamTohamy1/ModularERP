using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Treasuries.Commands;
using ModularERP.Modules.Finance.Features.Treasuries.Models;
using ModularERP.SharedKernel.Interfaces;

namespace ModularERP.Modules.Finance.Features.Treasuries.Handlers
{
    public class UpdateTreasuryHandler : IRequestHandler<UpdateTreasuryCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<Treasury> _treasuryRepository;
        private readonly IMapper _mapper;

        public UpdateTreasuryHandler(IGeneralRepository<Treasury> treasuryRepository, IMapper mapper)
        {
            _treasuryRepository = treasuryRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<bool>> Handle(UpdateTreasuryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var treasury = await _treasuryRepository
                    .Get(t => t.Id == request.Treasury.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (treasury == null)
                {
                    return ResponseViewModel<bool>.Error(
                        "Treasury not found",
                        FinanceErrorCode.TreasuryNotFound);
                }

                // Check if another treasury with same name exists for the company
                var existingTreasury = await _treasuryRepository
                    .Get(t => t.CompanyId == request.Treasury.CompanyId &&
                             t.Name == request.Treasury.Name &&
                             t.Id != request.Treasury.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingTreasury != null)
                {
                    return ResponseViewModel<bool>.Error(
                        "Treasury with this name already exists for the company",
                        FinanceErrorCode.TreasuryAlreadyExists);
                }

                _mapper.Map(request.Treasury, treasury);
                treasury.UpdatedAt = DateTime.UtcNow;

                await _treasuryRepository.Update(treasury);
                return ResponseViewModel<bool>.Success(true, "Treasury updated successfully");
            }
            catch (Exception ex)
            {
                return ResponseViewModel<bool>.Error(
                    "An error occurred while updating the treasury",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
