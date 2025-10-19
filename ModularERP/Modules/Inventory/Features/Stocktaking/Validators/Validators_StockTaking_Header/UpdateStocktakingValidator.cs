using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_Header;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Validators.Validators_StockTaking_Header
{
    public class UpdateStocktakingValidator : AbstractValidator<UpdateStocktakingCommand>
    {
        private readonly IGeneralRepository<Warehouse> _warehouseRepo;
        private readonly IGeneralRepository<StocktakingHeader> _stocktakingRepo;

        public UpdateStocktakingValidator(
            IGeneralRepository<Warehouse> warehouseRepo,
            IGeneralRepository<StocktakingHeader> stocktakingRepo)
        {
            _warehouseRepo = warehouseRepo;
            _stocktakingRepo = stocktakingRepo;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Stocktaking ID is required")
                .MustAsync(StocktakingExists).WithMessage("Stocktaking does not exist")
                .MustAsync(CanBeUpdated).WithMessage("Only Draft stocktakings can be updated");

            RuleFor(x => x.WarehouseId)
                .NotEmpty().WithMessage("Warehouse is required")
                .MustAsync(WarehouseExists).WithMessage("Warehouse does not exist");

            RuleFor(x => x.Number)
                .NotEmpty().WithMessage("Number is required")
                .MaximumLength(50).WithMessage("Number cannot exceed 50 characters")
                .MustAsync(NumberIsUnique).WithMessage("Number already exists");

            RuleFor(x => x.DateTime)
                .NotEmpty().WithMessage("Date time is required")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Date time cannot be in the future");
        }

        private async Task<bool> StocktakingExists(Guid id, CancellationToken cancellationToken)
        {
            return await _stocktakingRepo.AnyAsync(s => s.Id == id, cancellationToken);
        }

        private async Task<bool> CanBeUpdated(Guid id, CancellationToken cancellationToken)
        {
            var stocktaking = await _stocktakingRepo
                .Get(s => s.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            return stocktaking != null && stocktaking.Status == StocktakingStatus.Draft;
        }

        private async Task<bool> WarehouseExists(Guid warehouseId, CancellationToken cancellationToken)
        {
            return await _warehouseRepo.AnyAsync(w => w.Id == warehouseId, cancellationToken);
        }

        private async Task<bool> NumberIsUnique(UpdateStocktakingCommand command, string number, CancellationToken cancellationToken)
        {
            var exists = await _stocktakingRepo
                .Get(s => s.Number == number && s.Id != command.Id)
                .AnyAsync(cancellationToken);

            return !exists;
        }
    }
}