using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Inventory.Features.StockTransactions.DTO;
using ModularERP.Modules.Inventory.Features.StockTransactions.Models;
using ModularERP.Modules.Inventory.Features.StockTransactions.Qeuries;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.StockTransactions.Handlers
{
    public class GetAllStockTransactionsQueryHandler : IRequestHandler<GetAllStockTransactionsQuery, List<StockTransactionDto>>
    {
        private readonly IGeneralRepository<StockTransaction> _repository;
        private readonly IMapper _mapper;

        public GetAllStockTransactionsQueryHandler(IGeneralRepository<StockTransaction> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<StockTransactionDto>> Handle(GetAllStockTransactionsQuery request, CancellationToken cancellationToken)
        {
            var query = _repository.GetAll();

            if (request.CompanyId.HasValue)
                query = query.Where(x => x.CompanyId == request.CompanyId.Value);

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