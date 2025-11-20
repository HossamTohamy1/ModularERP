using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Modules.Purchases.Refunds.Qeuries.Qeuries_RefundInvoce;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Handlers.Handlers_RefundInvoce
{
    public class GetAllRefundsHandler : IRequestHandler<GetAllRefundsQuery, ResponseViewModel<List<RefundDto>>>
    {
        private readonly IGeneralRepository<PurchaseRefund> _refundRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllRefundsHandler> _logger;

        public GetAllRefundsHandler(
            IGeneralRepository<PurchaseRefund> refundRepo,
            IMapper mapper,
            ILogger<GetAllRefundsHandler> logger)
        {
            _refundRepo = refundRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<RefundDto>>> Handle(GetAllRefundsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving all refunds");

                var query = _refundRepo.GetAll();

                // Apply filters
                if (request.SupplierId.HasValue)
                {
                    query = query.Where(r => r.SupplierId == request.SupplierId.Value);
                }

                if (request.FromDate.HasValue)
                {
                    query = query.Where(r => r.RefundDate >= request.FromDate.Value);
                }

                if (request.ToDate.HasValue)
                {
                    query = query.Where(r => r.RefundDate <= request.ToDate.Value);
                }

                // Pagination
                var skip = (request.PageNumber - 1) * request.PageSize;
                query = query.Skip(skip).Take(request.PageSize);

                // Project to DTO using AutoMapper
                var refunds = await query
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => _mapper.Map<RefundDto>(r))
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {Count} refunds", refunds.Count);

                return ResponseViewModel<List<RefundDto>>.Success(refunds, "Refunds retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refunds");
                throw;
            }
        }
    }

    public class GetRefundByIdHandler : IRequestHandler<GetRefundByIdQuery, ResponseViewModel<RefundDto>>
    {
        private readonly IGeneralRepository<PurchaseRefund> _refundRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetRefundByIdHandler> _logger;

        public GetRefundByIdHandler(
            IGeneralRepository<PurchaseRefund> refundRepo,
            IMapper mapper,
            ILogger<GetRefundByIdHandler> logger)
        {
            _refundRepo = refundRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<RefundDto>> Handle(GetRefundByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving refund: {RefundId}", request.Id);

                var refund = await _refundRepo.GetAll()
                    .Where(r => r.Id == request.Id)
                    .Select(r => _mapper.Map<RefundDto>(r))
                    .FirstOrDefaultAsync(cancellationToken);

                if (refund == null)
                {
                    throw new NotFoundException(
                        $"Refund with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                _logger.LogInformation("Successfully retrieved refund: {RefundId}", request.Id);

                return ResponseViewModel<RefundDto>.Success(refund, "Refund retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refund: {RefundId}", request.Id);
                throw;
            }
        }
    }
}