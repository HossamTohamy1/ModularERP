using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRNItem
{
    public class AddGRNItemCommand : IRequest<ResponseViewModel<GRNLineItemResponseDto>>
    {
        public Guid GRNId { get; set; }
        public Guid POLineItemId { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public string? Notes { get; set; }
        public Guid UserId { get; set; }
    }
}