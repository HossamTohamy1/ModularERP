using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRN
{
    public class DeleteGRNCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }

        public DeleteGRNCommand(Guid id)
        {
            Id = id;
        }
    }
}
