using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Modules.Inventory.Features.Warehouses.DTO;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Validators;
using ModularERP.Shared.Interfaces;
using Serilog;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Handlers
{
    public class GetWarehouseByIdQueryHandler : IRequestHandler<GetWarehouseByIdQuery, WarehouseDto>
    {
        private readonly IGeneralRepository<Warehouse> _warehouseRepository;
        private readonly IMapper _mapper;

        public GetWarehouseByIdQueryHandler(
            IGeneralRepository<Warehouse> warehouseRepository,
            IMapper mapper)
        {
            _warehouseRepository = warehouseRepository;
            _mapper = mapper;
        }

        public async Task<WarehouseDto> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
        {
            Log.Information("Getting warehouse by ID: {WarehouseId}", request.Id);

            var warehouse = await _warehouseRepository
                .GetAll()
                .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

            if (warehouse == null)
            {
                Log.Warning("Warehouse not found: {WarehouseId}", request.Id);
                throw new NotFoundException(
                    $"Warehouse with ID {request.Id} not found",
                    FinanceErrorCode.NotFound);
            }

            Log.Information("Warehouse retrieved successfully: {WarehouseId}", warehouse.Id);

            return _mapper.Map<WarehouseDto>(warehouse);
        }
    }

}

