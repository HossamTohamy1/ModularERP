using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.TaxManagement.Commends;
using ModularERP.Modules.Inventory.Features.TaxManagement.Models;
using ModularERP.Shared.Interfaces;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Handlers
{
    public class DeleteTaxProfileHandler : IRequestHandler<DeleteTaxProfileCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<TaxProfile> _profileRepository;
        private readonly IJoinTableRepository<TaxProfileComponent> _componentRepository;
        private readonly FinanceDbContext _context;

        public DeleteTaxProfileHandler(
            IGeneralRepository<TaxProfile> profileRepository,
            IJoinTableRepository<TaxProfileComponent> componentRepository,
            FinanceDbContext context)
        {
            _profileRepository = profileRepository;
            _componentRepository = componentRepository;
            _context = context;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeleteTaxProfileCommand request, CancellationToken cancellationToken)
        {
            var profile = await _profileRepository.GetByID(request.Id);
            if (profile == null)
            {
                throw new NotFoundException("Tax profile not found", FinanceErrorCode.NotFound);
            }

            // Hard delete associated components
            var components = await _componentRepository
                .Get(pc => pc.TaxProfileId == request.Id)
                .ToListAsync(cancellationToken);

            if (components.Any())
            {
                _context.Set<TaxProfileComponent>().RemoveRange(components);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Soft delete the profile
            await _profileRepository.Delete(request.Id);
            await _profileRepository.SaveChanges();

            return ResponseViewModel<bool>.Success(true, "Tax profile deleted successfully");
        }
    }
}