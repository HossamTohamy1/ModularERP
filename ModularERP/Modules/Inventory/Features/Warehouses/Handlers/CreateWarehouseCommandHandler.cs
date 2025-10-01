using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Modules.Inventory.Features.Warehouses.Commends;
using ModularERP.Modules.Inventory.Features.Warehouses.DTO;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;
using Serilog;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Handlers
{
    public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, WarehouseDto>
    {
        private readonly IGeneralRepository<Warehouse> _warehouseRepository;
        private readonly IMapper _mapper;

        public CreateWarehouseCommandHandler(
            IGeneralRepository<Warehouse> warehouseRepository,
            IMapper mapper)
        {
            _warehouseRepository = warehouseRepository;
            _mapper = mapper;
        }

        public async Task<WarehouseDto> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Creating new warehouse: {Name} for CompanyId: {CompanyId}",
                request.Name, request.CompanyId);

            var existingWarehouse = await _warehouseRepository
                .GetByCompanyId(request.CompanyId)
                .FirstOrDefaultAsync(w => w.Name == request.Name, cancellationToken);

            if (existingWarehouse != null)
            {
                Log.Warning("Warehouse name already exists: {Name} for CompanyId: {CompanyId}",
                    request.Name, request.CompanyId);
                throw new BusinessLogicException(
                    "Warehouse name already exists in this company",
                    "Inventory",
                    FinanceErrorCode.DuplicateEntity);
            }

            if (request.IsPrimary)
            {
                var currentPrimary = await _warehouseRepository
                    .GetByCompanyId(request.CompanyId)
                    .FirstOrDefaultAsync(w => w.IsPrimary, cancellationToken);

                if (currentPrimary != null)
                {
                    Log.Information("Updating previous primary warehouse: {WarehouseId}", currentPrimary.Id);
                    currentPrimary.IsPrimary = false;
                    currentPrimary.UpdatedAt = DateTime.UtcNow;
                    await _warehouseRepository.Update(currentPrimary);
                }
            }

            var warehouse = _mapper.Map<Warehouse>(request);
            warehouse.Id = Guid.NewGuid();
            warehouse.CreatedAt = DateTime.UtcNow;
            warehouse.UpdatedAt = DateTime.UtcNow;

            await _warehouseRepository.AddAsync(warehouse);
            await _warehouseRepository.SaveChanges();

            Log.Information("Warehouse created successfully: {WarehouseId}", warehouse.Id);

            return _mapper.Map<WarehouseDto>(warehouse);
        }
    }
}


