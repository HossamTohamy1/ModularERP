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
    public class SetPrimaryWarehouseCommandHandler : IRequestHandler<SetPrimaryWarehouseCommand, WarehouseDto>
    {
        private readonly IGeneralRepository<Warehouse> _warehouseRepository;
        private readonly IMapper _mapper;

        public SetPrimaryWarehouseCommandHandler(
            IGeneralRepository<Warehouse> warehouseRepository,
            IMapper mapper)
        {
            _warehouseRepository = warehouseRepository;
            _mapper = mapper;
        }

        public async Task<WarehouseDto> Handle(SetPrimaryWarehouseCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Setting warehouse as primary: {WarehouseId}", request.Id);

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

            // Warehouse must be active to be set as primary
            if (warehouse.Status == WarehouseStatus.Inactive)
            {
                Log.Warning("Cannot set inactive warehouse as primary: {WarehouseId}", request.Id);
                throw new BusinessLogicException(
                    "Cannot set inactive warehouse as primary. Please activate the warehouse first",
                    "Inventory",
                    FinanceErrorCode.BusinessLogicError);
            }

            // If already primary, return as is
            if (warehouse.IsPrimary)
            {
                Log.Information("Warehouse is already primary: {WarehouseId}", request.Id);
                return _mapper.Map<WarehouseDto>(warehouse);
            }

            // Remove primary status from current primary warehouse
            var currentPrimary = await _warehouseRepository
                .GetByCompanyId(request.CompanyId)
                .FirstOrDefaultAsync(w => w.IsPrimary, cancellationToken);

            if (currentPrimary != null)
            {
                Log.Information("Removing primary status from warehouse: {WarehouseId}", currentPrimary.Id);
                currentPrimary.IsPrimary = false;
                currentPrimary.UpdatedAt = DateTime.UtcNow;
                await _warehouseRepository.Update(currentPrimary);
            }

            // Set new primary warehouse
            warehouse.IsPrimary = true;
            warehouse.UpdatedAt = DateTime.UtcNow;

            await _warehouseRepository.Update(warehouse);
            await _warehouseRepository.SaveChanges();

            Log.Information("Warehouse set as primary successfully: {WarehouseId}", warehouse.Id);

            return _mapper.Map<WarehouseDto>(warehouse);
        }
    }
}

