using MediatR;
using ModularERP.Modules.Inventory.Features.Warehouses.DTO;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Validators
{
    public class GetWarehousesQuery : IRequest<PagedResult<WarehouseListDto>>
    {
        public Guid? CompanyId { get; set; }
        public string? Status { get; set; }
        public bool? IsPrimary { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetWarehouseByIdQuery : IRequest<WarehouseDto>
    {
        public Guid Id { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}

