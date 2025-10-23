using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockSnapshot;
using ModularERP.Shared.Interfaces;
using System.Text;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockSnapshot
{
    public class ExportSnapshotQueryHandler : IRequestHandler<ExportSnapshotQuery, ResponseViewModel<byte[]>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _stocktakingRepository;
        private readonly ILogger<ExportSnapshotQueryHandler> _logger;

        public ExportSnapshotQueryHandler(
            IGeneralRepository<StocktakingHeader> stocktakingRepository,
            ILogger<ExportSnapshotQueryHandler> logger)
        {
            _stocktakingRepository = stocktakingRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<byte[]>> Handle(ExportSnapshotQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Exporting snapshot for stocktaking {StocktakingId} in {Format} format",
                    request.StocktakingId, request.Format);

                var stocktaking = await _stocktakingRepository
                    .GetAll()
                    .Where(s => s.Id == request.StocktakingId)
                    .Select(s => new
                    {
                        s.Number,
                        s.DateTime,
                        Snapshots = s.Snapshots.Select(snap => new
                        {
                            ProductName = snap.Product.Name,
                            ProductSKU = snap.Product.SKU,
                            snap.QtyAtStart,
                            snap.CreatedAt
                        }).ToList()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (stocktaking == null)
                {
                    throw new NotFoundException(
                        $"Stocktaking with ID {request.StocktakingId} not found",
                        FinanceErrorCode.NotFound);
                }

                // Generate CSV format
                var csv = new StringBuilder();
                csv.AppendLine("Product Name,Product SKU,Snapshot Quantity,Snapshot Date");

                foreach (var snapshot in stocktaking.Snapshots)
                {
                    csv.AppendLine($"{snapshot.ProductName},{snapshot.ProductSKU},{snapshot.QtyAtStart},{snapshot.CreatedAt:yyyy-MM-dd HH:mm}");
                }

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());

                _logger.LogInformation("Successfully exported {Count} snapshots", stocktaking.Snapshots.Count);

                return ResponseViewModel<byte[]>.Success(bytes, "Snapshot exported successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting snapshot");
                throw new BusinessLogicException(
                    "Failed to export snapshot",
                    "Inventory",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }

}
