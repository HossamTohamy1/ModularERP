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
    public class GetStockTransactionByIdQueryHandler : IRequestHandler<GetStockTransactionByIdQuery, StockTransactionDto>
    {
        private readonly IGeneralRepository<StockTransaction> _repository;
        private readonly IMapper _mapper;

        public GetStockTransactionByIdQueryHandler(IGeneralRepository<StockTransaction> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<StockTransactionDto> Handle(GetStockTransactionByIdQuery request, CancellationToken cancellationToken)
        {
            var transaction = await _repository.GetAll()
                .Where(x => x.Id == request.Id)
                .ProjectTo<StockTransactionDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (transaction == null)
                throw new NotFoundException("Stock transaction not found", FinanceErrorCode.NotFound);

            return transaction;
        }
    }

}
