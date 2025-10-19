using FluentValidation;
using ModularERP.Modules.Finance.Features.Companys.Models;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_Header;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Validators.Validators_StockTaking_Header
{
    public class CreateStocktakingValidator : AbstractValidator<CreateStocktakingCommand>
    {
        private readonly IGeneralRepository<Warehouse> _warehouseRepo;
        private readonly IGeneralRepository<Company> _companyRepo;

        public CreateStocktakingValidator(
            IGeneralRepository<Warehouse> warehouseRepo,
            IGeneralRepository<Company> companyRepo)
        {
            _warehouseRepo = warehouseRepo;
            _companyRepo = companyRepo;

            RuleFor(x => x.WarehouseId)
                .NotEmpty().WithMessage("Warehouse is required")
                .MustAsync(WarehouseExists).WithMessage("Warehouse does not exist");

            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("Company is required")
                .MustAsync(CompanyExists).WithMessage("Company does not exist");

            RuleFor(x => x.Number)
                .NotEmpty().WithMessage("Number is required")
                .MaximumLength(50).WithMessage("Number cannot exceed 50 characters");

            RuleFor(x => x.DateTime)
                .NotEmpty().WithMessage("Date time is required")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Date time cannot be in the future");
        }

        private async Task<bool> WarehouseExists(Guid warehouseId, CancellationToken cancellationToken)
        {
            return await _warehouseRepo.AnyAsync(w => w.Id == warehouseId, cancellationToken);
        }

        private async Task<bool> CompanyExists(Guid companyId, CancellationToken cancellationToken)
        {
            return await _companyRepo.AnyAsync(c => c.Id == companyId, cancellationToken);
        }
    }
}