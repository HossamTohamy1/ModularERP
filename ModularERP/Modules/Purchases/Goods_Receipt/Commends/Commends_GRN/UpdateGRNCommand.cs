using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRN
{
    public class UpdateGRNCommand : IRequest<ResponseViewModel<GRNResponseDto>>
    {
        public UpdateGRNDto Data { get; set; } = null!;

        public UpdateGRNCommand(UpdateGRNDto data)
        {
            Data = data;
        }
    }
}
