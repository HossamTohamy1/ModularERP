using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRN
{
    public class CreateGRNCommand : IRequest<ResponseViewModel<GRNResponseDto>>
    {
        public CreateGRNDto Data { get; set; } = null!;

        public CreateGRNCommand(CreateGRNDto data)
        {
            Data = data;
        }
    }
}
