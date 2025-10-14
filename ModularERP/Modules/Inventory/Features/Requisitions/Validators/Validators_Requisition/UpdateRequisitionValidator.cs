using FluentValidation;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_Requisition;
using ModularERP.Modules.Inventory.Features.Requisitions.Models;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Validators.Validators_Requisition
{
    public class UpdateRequisitionValidator : AbstractValidator<UpdateRequisitionCommand>
    {
        private readonly IGeneralRepository<Warehouse> _warehouseRepo;
        private readonly IGeneralRepository<Product> _productRepo;
        private readonly IGeneralRepository<Supplier> _supplierRepo;
        private readonly IGeneralRepository<Requisition> _requisitionRepo;

        public UpdateRequisitionValidator(
            IGeneralRepository<Warehouse> warehouseRepo,
            IGeneralRepository<Product> productRepo,
            IGeneralRepository<Supplier> supplierRepo,
            IGeneralRepository<Requisition> requisitionRepo)
        {
            _warehouseRepo = warehouseRepo;
            _productRepo = productRepo;
            _supplierRepo = supplierRepo;
            _requisitionRepo = requisitionRepo;

            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Requisition ID is required.")
                .MustAsync(RequisitionExists)
                .WithMessage("Requisition does not exist.")
                .MustAsync(RequisitionIsEditable)
                .WithMessage("Only Draft requisitions can be edited.");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid requisition type.");

            RuleFor(x => x.Date)
                .NotEmpty()
                .WithMessage("Date is required.");

            RuleFor(x => x.WarehouseId)
                .NotEmpty()
                .WithMessage("Warehouse is required.")
                .MustAsync(WarehouseExists)
                .WithMessage("Warehouse does not exist.");

            RuleFor(x => x.CompanyId)
                .NotEmpty()
                .WithMessage("Company is required.");

            RuleFor(x => x.SupplierId)
                .MustAsync(SupplierExistsIfProvided)
                .When(x => x.SupplierId.HasValue)
                .WithMessage("Supplier does not exist.");

            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("At least one item is required.")
                .Must(items => items.Count > 0)
                .WithMessage("At least one item is required.");

            RuleForEach(x => x.Items).SetValidator(new RequisitionItemValidator(_productRepo));

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .WithMessage("Notes cannot exceed 1000 characters.");
        }

        private async Task<bool> RequisitionExists(Guid id, CancellationToken cancellationToken)
        {
            return await _requisitionRepo.AnyAsync(r => r.Id == id);
        }

        private async Task<bool> RequisitionIsEditable(Guid id, CancellationToken cancellationToken)
        {
            var requisition = await _requisitionRepo.GetByID(id);
            return requisition?.Status == RequisitionStatus.Draft;
        }

        private async Task<bool> WarehouseExists(Guid warehouseId, CancellationToken cancellationToken)
        {
            return await _warehouseRepo.AnyAsync(w => w.Id == warehouseId);
        }

        private async Task<bool> SupplierExistsIfProvided(Guid? supplierId, CancellationToken cancellationToken)
        {
            if (!supplierId.HasValue) return true;
            return await _supplierRepo.AnyAsync(s => s.Id == supplierId.Value);
        }
    }
}