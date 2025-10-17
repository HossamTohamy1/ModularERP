using MediatR;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ProductStats;

namespace ModularERP.Modules.Inventory.Features.Products.Qeuries.Qeuries_ProductStats
{
    public class RefreshProductStatsCommand : IRequest<RefreshStatsResultDto>
    {
        public Guid ProductId { get; set; }
        public Guid CompanyId { get; set; }
    }
}
