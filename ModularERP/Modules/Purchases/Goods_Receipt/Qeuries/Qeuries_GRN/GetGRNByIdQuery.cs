using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Qeuries.Qeuries_GRN
{
    public class GetGRNByIdQuery : IRequest<ResponseViewModel<GRNResponseDto>>
    {
        public Guid Id { get; set; }

        public GetGRNByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
