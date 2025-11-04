using MediatR;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRN
{
    public class PostGRNCommand : IRequest<GRNResponseDto>
    {
        public Guid GRNId { get; set; }
        public Guid UserId { get; set; }
    }
}