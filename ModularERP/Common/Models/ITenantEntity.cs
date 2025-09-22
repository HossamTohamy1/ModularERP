namespace ModularERP.Common.Models
{
    public interface ITenantEntity
    {
        Guid CompanyId { get; set; }
        bool IsDeleted { get; set; }

    }
}
