using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTraking_WorkFlow
{
    public class GetStocktakingDetailQuery : IRequest<ResponseViewModel<StocktakingDetailDto>>
    {
        public Guid StocktakingId { get; set; }
        public Guid CompanyId { get; set; }
    }

}
