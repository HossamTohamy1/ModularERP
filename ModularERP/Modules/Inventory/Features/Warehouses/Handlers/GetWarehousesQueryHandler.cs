using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Modules.Inventory.Features.Warehouses.DTO;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Validators;
using ModularERP.Shared.Interfaces;
using Serilog;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Handlers
{
    public class GetWarehousesQueryHandler : IRequestHandler<GetWarehousesQuery, PagedResult<WarehouseListDto>>
    {
        private readonly IGeneralRepository<Warehouse> _warehouseRepository;
        private readonly IMapper _mapper;

        public GetWarehousesQueryHandler(
            IGeneralRepository<Warehouse> warehouseRepository,
            IMapper mapper)
        {
            _warehouseRepository = warehouseRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<WarehouseListDto>> Handle(
            GetWarehousesQuery request,
            CancellationToken cancellationToken)
        {
            Log.Information(
                "Getting warehouses with filters: CompanyId={CompanyId}, Status={Status}, IsPrimary={IsPrimary}, SearchTerm={SearchTerm}, PageNumber={PageNumber}, PageSize={PageSize}",
                request.CompanyId, request.Status, request.IsPrimary, request.SearchTerm,
                request.PageNumber, request.PageSize);

            var query = _warehouseRepository.GetAll();

            if (request.CompanyId.HasValue)
            {
                query = query.Where(w => w.CompanyId == request.CompanyId.Value);
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                if (Enum.TryParse<WarehouseStatus>(request.Status, out var status))
                {
                    query = query.Where(w => w.Status == status);
                }
                else
                {
                    throw new BusinessLogicException(
                        "Invalid warehouse status",
                        "Inventory",
                        FinanceErrorCode.ValidationError);
                }
            }

            if (request.IsPrimary.HasValue)
            {
                query = query.Where(w => w.IsPrimary == request.IsPrimary.Value);
            }

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(w =>
                    w.Name.ToLower().Contains(searchLower) ||
                    (w.ShippingAddress != null && w.ShippingAddress.ToLower().Contains(searchLower)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            if (request.PageNumber < 1)
            {
                throw new BusinessLogicException(
                    "Page number must be greater than zero",
                    "Inventory",
                    FinanceErrorCode.ValidationError);
            }

            if (request.PageSize < 1 || request.PageSize > 100)
            {
                throw new BusinessLogicException(
                    "Page size must be between 1 and 100",
                    "Inventory",
                    FinanceErrorCode.ValidationError);
            }

            var warehouses = await query
                .OrderByDescending(w => w.IsPrimary)
                .ThenBy(w => w.Name)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            Log.Information(
                "Retrieved {Count} warehouses out of {TotalCount} (Page {PageNumber}/{TotalPages})",
                warehouses.Count,
                totalCount,
                request.PageNumber,
                (int)Math.Ceiling(totalCount / (double)request.PageSize));

            // استخدام AutoMapper لإنشاء PagedResult
            return _mapper.Map<PagedResult<WarehouseListDto>>(
                new { Warehouses = warehouses, TotalCount = totalCount, request.PageNumber, request.PageSize });
        }
    }
}
