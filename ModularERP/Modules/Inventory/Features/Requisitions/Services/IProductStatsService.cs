namespace ModularERP.Modules.Inventory.Features.Requisitions.Services
{
    public interface IProductStatsService
    {
        Task UpdateProductStats(Guid productId, Guid companyId, CancellationToken cancellationToken = default);

    }
}
