using MediatR;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_ImportExport;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_ImportExport
{
    public class UploadStocktakingLineImageCommand : IRequest<UploadImageResultDto>
    {
        public Guid StocktakingId { get; set; }
        public Guid LineId { get; set; }
        public IFormFile Image { get; set; } = null!;
    }
}