using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commands_StockTraking_WorkFlow
{
    public class CreateStocktakingCommand : IRequest<ResponseViewModel<CreateStocktakingDto>>
    {
        public Guid WarehouseId { get; set; }
        public Guid CompanyId { get; set; }
        [MaxLength(50)]
        public string Number { get; set; }
        public DateTime DateTime { get; set; }
        public string Notes { get; set; }
        public Guid CreatedByUserId { get; set; }
    }

}
