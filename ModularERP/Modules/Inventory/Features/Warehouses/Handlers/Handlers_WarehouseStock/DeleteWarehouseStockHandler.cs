using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Warehouses.Commends.Commends_WarehouseStock;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;
using Serilog;
using ILogger = Serilog.ILogger;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Handlers.Handlers_WarehouseStock
{
    public class DeleteWarehouseStockHandler : IRequestHandler<DeleteWarehouseStockCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<WarehouseStock> _repository;
        private readonly ILogger _logger;

        public DeleteWarehouseStockHandler(IGeneralRepository<WarehouseStock> repository)
        {
            _repository = repository;
            _logger = Log.ForContext<DeleteWarehouseStockHandler>();
        }

        public async Task<ResponseViewModel<bool>> Handle(
            DeleteWarehouseStockCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.Information("Deleting warehouse stock with Id: {Id}", request.Id);

                var entity = await _repository.GetByID(request.Id);
                if (entity == null || entity.IsDeleted)
                {
                    _logger.Warning("Warehouse stock not found with Id: {Id}", request.Id);
                    throw new NotFoundException(
                        $"Warehouse stock with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedById = Guid.Empty; // TODO: Get from current user

                _repository.Update(entity);
                await _repository.SaveChanges();

                _logger.Information("Warehouse stock deleted successfully with Id: {Id}", request.Id);

                return ResponseViewModel<bool>.Success(true, "Warehouse stock deleted successfully");
            }
            catch (Exception ex) when (ex is not BusinessLogicException && ex is not NotFoundException)
            {
                _logger.Error(ex, "Error deleting warehouse stock with Id: {Id}", request.Id);
                throw;
            }
        }
    }
}
