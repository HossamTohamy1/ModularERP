using MediatR;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRNItem
{
    public class UpdateGRNItemCommand : IRequest<GRNLineItemResponseDto>
    {
        public Guid GRNId { get; set; }
        public Guid ItemId { get; set; }
        public Guid POLineItemId { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public string? Notes { get; set; }
        public Guid UserId { get; set; }
    }
}