using MediatR;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRNItem
{
    public class DeleteGRNItemCommand : IRequest<Unit>
    {
        public Guid GRNId { get; set; }
        public Guid ItemId { get; set; }
    }
}