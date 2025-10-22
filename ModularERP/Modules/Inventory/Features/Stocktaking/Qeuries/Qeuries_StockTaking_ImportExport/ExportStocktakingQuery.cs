using MediatR;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_ImportExport;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTaking_ImportExport
{
    public class ExportStocktakingQuery : IRequest<ExportStocktakingResultDto>
    {
        public Guid StocktakingId { get; set; }
    }

}
