using MediatR;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTraking_WorkFlow
{
    public class GetAllStocktakingsQuery : IRequest<ResponseViewModel<List<StocktakingListDto>>>
    {
        public Guid CompanyId { get; set; }
        public Guid? WarehouseId { get; set; }
        public StocktakingStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
