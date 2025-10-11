using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries;
using ModularERP.Modules.Inventory.Features.StockTransactions.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers
{
    public class GetStockTransactionsByProductQueryHandler : IRequestHandler<GetStockTransactionsByProductQuery, List<StockTransactionDto>>
    {
        private readonly IGeneralRepository<StockTransaction> _repository;
        private readonly IMapper _mapper;

        public GetStockTransactionsByProductQueryHandler(IGeneralRepository<StockTransaction> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<StockTransactionDto>> Handle(GetStockTransactionsByProductQuery request, CancellationToken cancellationToken)
        {
            var query = _repository.GetAll()
                .Where(x => x.ProductId == request.ProductId);

            if (request.WarehouseId.HasValue)
                query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);

            if (request.FromDate.HasValue)
                query = query.Where(x => x.CreatedAt >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                query = query.Where(x => x.CreatedAt <= request.ToDate.Value);

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .ProjectTo<StockTransactionDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
