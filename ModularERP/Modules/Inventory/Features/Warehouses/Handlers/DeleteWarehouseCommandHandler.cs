using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Inventory.Features.Warehouses.Commends;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;
using Serilog;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Handlers
{
    public class DeleteWarehouseCommandHandler : IRequestHandler<DeleteWarehouseCommand, bool>
    {
        private readonly IGeneralRepository<Warehouse> _warehouseRepository;

        public DeleteWarehouseCommandHandler(IGeneralRepository<Warehouse> warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<bool> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Deleting warehouse: {WarehouseId}", request.Id);

            var warehouse = await _warehouseRepository
                .Get(w => w.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);


            if (warehouse == null)
            {
                Log.Warning("Warehouse not found: {WarehouseId}", request.Id);
                throw new NotFoundException(
                    $"Warehouse with ID {request.Id} not found",
                    FinanceErrorCode.NotFound);
            }

            // Prevent deleting primary warehouse
            if (warehouse.IsPrimary)
            {
                Log.Warning("Cannot delete primary warehouse: {WarehouseId}", request.Id);
                throw new BusinessLogicException(
                    "Cannot delete primary warehouse. Please set another warehouse as primary first",
                    "Inventory",
                    FinanceErrorCode.BusinessLogicError);
            }



            await _warehouseRepository.Delete(request.Id);
            await _warehouseRepository.SaveChanges();

            Log.Information("Warehouse deleted successfully: {WarehouseId}", request.Id);

            return true;
        }
    }
}
