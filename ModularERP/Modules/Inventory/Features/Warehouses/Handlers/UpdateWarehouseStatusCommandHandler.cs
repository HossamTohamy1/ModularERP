using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Inventory.Features.Warehouses.Commends;
using ModularERP.Modules.Inventory.Features.Warehouses.DTO;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;
using Serilog;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Handlers
{
    public class UpdateWarehouseStatusCommandHandler : IRequestHandler<UpdateWarehouseStatusCommand, WarehouseDto>
    {
        private readonly IGeneralRepository<Warehouse> _warehouseRepository;
        private readonly IMapper _mapper;

        public UpdateWarehouseStatusCommandHandler(
            IGeneralRepository<Warehouse> warehouseRepository,
            IMapper mapper)
        {
            _warehouseRepository = warehouseRepository;
            _mapper = mapper;
        }

        public async Task<WarehouseDto> Handle(UpdateWarehouseStatusCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Updating warehouse status: {WarehouseId} to {Status}", request.Id, request.Status);

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

            // Prevent deactivating primary warehouse
            if (warehouse.IsPrimary && request.Status == "Inactive")
            {
                Log.Warning("Cannot deactivate primary warehouse: {WarehouseId}", request.Id);
                throw new BusinessLogicException(
                    "Cannot deactivate primary warehouse. Please set another warehouse as primary first",
                    "Inventory",
                    FinanceErrorCode.BusinessLogicError);
            }

            warehouse.Status = Enum.Parse<WarehouseStatus>(request.Status);
            warehouse.UpdatedAt = DateTime.UtcNow;

            await _warehouseRepository.Update(warehouse);
            await _warehouseRepository.SaveChanges();

            Log.Information("Warehouse status updated successfully: {WarehouseId}", warehouse.Id);

            return _mapper.Map<WarehouseDto>(warehouse);
        }
    }
}
