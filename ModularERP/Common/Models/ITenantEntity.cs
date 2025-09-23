namespace ModularERP.Common.Models
{
    public interface ITenantEntity
    {
        Guid TenantId { get; set; }
        bool IsDeleted { get; set; }

    }
}
