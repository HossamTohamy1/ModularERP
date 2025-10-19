using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commands_StockTraking_WorkFlow
{
    public class UpdateStocktakingCommand : IRequest<ResponseViewModel<UpdateStocktakingDto>>
    {
        public Guid StocktakingId { get; set; }
        public Guid CompanyId { get; set; }
        public string Notes { get; set; }
        public bool UpdateSystem { get; set; }
        public Guid UpdatedByUserId { get; set; }
    }
}
