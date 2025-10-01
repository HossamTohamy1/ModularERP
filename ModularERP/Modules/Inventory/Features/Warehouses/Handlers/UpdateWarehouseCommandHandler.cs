using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Inventory.Features.Warehouses.Commends;
using ModularERP.Modules.Inventory.Features.Warehouses.DTO;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;
using Serilog;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Handlers
{
    public class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand, WarehouseDto>
    {
        private readonly IGeneralRepository<Warehouse> _warehouseRepository;
        private readonly IMapper _mapper;

        public UpdateWarehouseCommandHandler(
            IGeneralRepository<Warehouse> warehouseRepository,
            IMapper mapper)
        {
            _warehouseRepository = warehouseRepository;
            _mapper = mapper;
        }

        public async Task<WarehouseDto> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Log.Information("Starting UpdateWarehouse for Id: {WarehouseId}, CompanyId: {CompanyId}",
                    request.Id, request.CompanyId);

                // استعلام المستودع
                var lookupQuery = _warehouseRepository
                    .GetByCompanyId(request.CompanyId)
                    .Where(w => w.Id == request.Id);

                Log.Debug("Generated SQL for warehouse lookup: {Sql}", lookupQuery.ToQueryString());

                var warehouse = await lookupQuery.FirstOrDefaultAsync(cancellationToken);

                if (warehouse == null)
                {
                    Log.Warning("Warehouse not found with Id: {WarehouseId} and CompanyId: {CompanyId}",
                        request.Id, request.CompanyId);

                    throw new NotFoundException(
                        $"Warehouse with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                Log.Information("Warehouse found. Proceeding with validation. Name: {WarehouseName}, IsPrimary: {IsPrimary}",
                    warehouse.Name, warehouse.IsPrimary);

                // التحقق من التكرار
                var duplicateWarehouse = await _warehouseRepository
                    .GetByCompanyId(request.CompanyId)
                    .FirstOrDefaultAsync(
                        w => w.Name == request.Name && w.Id != request.Id,
                        cancellationToken);

                if (duplicateWarehouse != null)
                {
                    Log.Warning("Duplicate warehouse name detected: {Name}", request.Name);

                    throw new BusinessLogicException(
                        "Warehouse name already exists in this company",
                        "Inventory",
                        FinanceErrorCode.DuplicateEntity);
                }

                Log.Information("Validation passed. Updating warehouse.");

                // لو تم تعيينه Primary جديد
                if (request.IsPrimary && !warehouse.IsPrimary)
                {
                    Log.Information("Checking for current primary warehouse...");

                    var currentPrimary = await _warehouseRepository
                        .GetByCompanyId(request.CompanyId)
                        .FirstOrDefaultAsync(
                            w => w.IsPrimary && w.Id != request.Id,
                            cancellationToken);

                    if (currentPrimary != null)
                    {
                        Log.Information("Removing primary from warehouse: {WarehouseId}", currentPrimary.Id);

                        currentPrimary.IsPrimary = false;
                        currentPrimary.UpdatedAt = DateTime.UtcNow;

                        await _warehouseRepository.Update(currentPrimary);
                    }
                }

                // التحديث باستخدام AutoMapper
                Log.Debug("Applying AutoMapper updates to warehouse: {WarehouseId}", warehouse.Id);
                _mapper.Map(request, warehouse);
                warehouse.UpdatedAt = DateTime.UtcNow;

                // الحفظ في قاعدة البيانات
                Log.Information("Saving changes to database...");
                await _warehouseRepository.Update(warehouse);
                await _warehouseRepository.SaveChanges();

                Log.Information("Warehouse updated successfully: {WarehouseId}", warehouse.Id);

                return _mapper.Map<WarehouseDto>(warehouse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating warehouse {WarehouseId} for CompanyId {CompanyId}",
                    request.Id, request.CompanyId);
                throw;
            }
        }
    }
}
