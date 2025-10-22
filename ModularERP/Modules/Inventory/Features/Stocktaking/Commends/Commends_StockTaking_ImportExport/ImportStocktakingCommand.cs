using MediatR;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_ImportExport;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_ImportExport
{
    public class ImportStocktakingCommand : IRequest<ImportStocktakingResultDto>
    {
        public Guid StocktakingId { get; set; }
        public IFormFile File { get; set; } = null!;
    }
}