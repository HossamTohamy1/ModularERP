using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ProductStats;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Modules.Inventory.Features.Products.Qeuries.Qeuries_ProductStats;
using ModularERP.Modules.Inventory.Features.StockTransactions.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Products.Handlers.Handlers_ProductStats
{
    public class RefreshProductStatsCommandHandler : IRequestHandler<RefreshProductStatsCommand, RefreshStatsResultDto>
    {
        private readonly IGeneralRepository<ProductStats> _statsRepository;
        private readonly IGeneralRepository<StockTransaction> _transactionRepository;
        private readonly IMapper _mapper;

        public RefreshProductStatsCommandHandler(
            IGeneralRepository<ProductStats> statsRepository,
            IGeneralRepository<StockTransaction> transactionRepository,
            IMapper mapper)
        {
            _statsRepository = statsRepository;
            _transactionRepository = transactionRepository;
            _mapper = mapper;
        }

        public async Task<RefreshStatsResultDto> Handle(RefreshProductStatsCommand request, CancellationToken cancellationToken)
        {
            var existingStats = await _statsRepository
                .GetByCompanyId(request.CompanyId)
                .FirstOrDefaultAsync(ps => ps.ProductId == request.ProductId, cancellationToken);

            if (existingStats == null)
            {
                throw new NotFoundException(
                    $"Product stats not found for ProductId: {request.ProductId}",
                    FinanceErrorCode.NotFound
                );
            }

            // Calculate dates for time-based statistics
            var now = DateTime.UtcNow;
            var last7DaysDate = now.AddDays(-7);
            var last28DaysDate = now.AddDays(-28);

            // Get transactions for calculations
            var transactions = await _transactionRepository
                .GetByCompanyId(request.CompanyId)
                .Where(t => t.ProductId == request.ProductId)
                .Select(t => new
                {
                    t.TransactionType,
                    t.Quantity,
                    t.UnitCost,
                    t.CreatedAt,
                    t.StockLevelAfter
                })
                .ToListAsync(cancellationToken);

            // Calculate statistics
            var salesTransactions = transactions.Where(t => t.TransactionType == StockTransactionType.Sale);

            existingStats.TotalSold = salesTransactions.Sum(t => Math.Abs(t.Quantity));
            existingStats.SoldLast28Days = salesTransactions
                .Where(t => t.CreatedAt >= last28DaysDate)
                .Sum(t => Math.Abs(t.Quantity));
            existingStats.SoldLast7Days = salesTransactions
                .Where(t => t.CreatedAt >= last7DaysDate)
                .Sum(t => Math.Abs(t.Quantity));

            // Calculate on-hand stock (latest transaction's StockLevelAfter)
            var latestTransaction = transactions.OrderByDescending(t => t.CreatedAt).FirstOrDefault();
            existingStats.OnHandStock = latestTransaction?.StockLevelAfter ?? 0;

            // Calculate weighted average unit cost
            var inboundTransactions = transactions
                .Where(t => t.TransactionType == StockTransactionType.Purchase || t.TransactionType == StockTransactionType .Adjustment&& t.Quantity > 0)
                .ToList();

            if (inboundTransactions.Any())
            {
                var totalCost = inboundTransactions.Sum(t => (t.Quantity) * (t.UnitCost ?? 0));
                var totalQty = inboundTransactions.Sum(t => t.Quantity );

                existingStats.AvgUnitCost = totalQty > 0 ? totalCost / totalQty : 0;

            }

            existingStats.LastUpdated = DateTime.UtcNow;
            existingStats.UpdatedAt = DateTime.UtcNow;

            await _statsRepository.Update(existingStats);
            await _statsRepository.SaveChanges();

            var updatedStatsDto = await _statsRepository
                .GetByCompanyId(request.CompanyId)
                .Where(ps => ps.Id == existingStats.Id)
                .ProjectTo<ProductStatsDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            return new RefreshStatsResultDto
            {
                ProductId = request.ProductId,
                Success = true,
                Message = "Product statistics refreshed successfully",
                RefreshedAt = DateTime.UtcNow,
                UpdatedStats = updatedStatsDto
            };
        }
    }

}
