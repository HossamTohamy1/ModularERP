using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Purchases_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Qeuries.Qeuries_PurchaseOrder;
using ModularERP.Shared.Interfaces;
using Serilog;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_PurchaseOrder
{
    public class GetPurchaseOrderListHandler : IRequestHandler<GetPurchaseOrderListQuery, ResponseViewModel<PagedResult<PurchaseOrderListDto>>>
    {
        private readonly IGeneralRepository<PurchaseOrder> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPurchaseOrderListHandler> _logger;

        public GetPurchaseOrderListHandler(
            IGeneralRepository<PurchaseOrder> repository,
            IMapper mapper,
            ILogger<GetPurchaseOrderListHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PagedResult<PurchaseOrderListDto>>> Handle(
            GetPurchaseOrderListQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving purchase orders list with filters");

                var query = _repository.GetAll();

                // Apply filters
                if (request.CompanyId.HasValue)
                    query = query.Where(x => x.CompanyId == request.CompanyId.Value);

                if (request.SupplierId.HasValue)
                    query = query.Where(x => x.SupplierId == request.SupplierId.Value);

                if (!string.IsNullOrEmpty(request.DocumentStatus))
                {
                    if (Enum.TryParse<DocumentStatus>(request.DocumentStatus, true, out var parsedStatus))
                    {
                        query = query.Where(x => x.DocumentStatus == parsedStatus);
                    }
                    else
                    {
                        throw new BusinessLogicException(
                            $"Invalid DocumentStatus value: {request.DocumentStatus}",
                            "Purchases");
                    }
                }

                if (!string.IsNullOrEmpty(request.ReceptionStatus))
                {
                    if (Enum.TryParse<ReceptionStatus>(request.ReceptionStatus, true, out var parsedReception))
                    {
                        query = query.Where(x => x.ReceptionStatus == parsedReception);
                    }
                    else
                    {
                        throw new BusinessLogicException(
                            $"Invalid ReceptionStatus value: {request.ReceptionStatus}",
                            "Purchases");
                    }
                }


                if (!string.IsNullOrEmpty(request.PaymentStatus))
                {
                    if (Enum.TryParse<PaymentStatus>(request.PaymentStatus, true, out var parsedPayment))
                    {
                        query = query.Where(x => x.PaymentStatus == parsedPayment);
                    }
                    else
                    {
                        throw new BusinessLogicException(
                            $"Invalid PaymentStatus value: {request.PaymentStatus}",
                            "Purchases");
                    }
                }


                if (request.FromDate.HasValue)
                    query = query.Where(x => x.PODate >= request.FromDate.Value);

                if (request.ToDate.HasValue)
                    query = query.Where(x => x.PODate <= request.ToDate.Value);

                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    var searchLower = request.SearchTerm.ToLower();
                    query = query.Where(x =>
                        x.PONumber.ToLower().Contains(searchLower) ||
                        x.Notes.ToLower().Contains(searchLower));
                }

                // Get total count before pagination
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply sorting
                query = ApplySorting(query, request.SortBy, request.SortDescending);

                // Apply pagination
                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ProjectTo<PurchaseOrderListDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

                var result = new PagedResult<PurchaseOrderListDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = totalPages,
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < totalPages
                };

                _logger.LogInformation("Retrieved {Count} purchase orders (Page {PageNumber} of {TotalPages})",
                    items.Count, request.PageNumber, totalPages);

                return ResponseViewModel<PagedResult<PurchaseOrderListDto>>.Success(
                    result,
                    $"Retrieved {items.Count} purchase orders");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase orders list");
                throw;
            }
        }

        private IQueryable<PurchaseOrder> ApplySorting(
            IQueryable<PurchaseOrder> query,
            string? sortBy,
            bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                sortBy = "CreatedAt";

            return sortBy.ToLower() switch
            {
                "ponumber" => descending ? query.OrderByDescending(x => x.PONumber) : query.OrderBy(x => x.PONumber),
                "podate" => descending ? query.OrderByDescending(x => x.PODate) : query.OrderBy(x => x.PODate),
                "totalamount" => descending ? query.OrderByDescending(x => x.TotalAmount) : query.OrderBy(x => x.TotalAmount),
                "documentstatus" => descending ? query.OrderByDescending(x => x.DocumentStatus) : query.OrderBy(x => x.DocumentStatus),
                "receptionstatus" => descending ? query.OrderByDescending(x => x.ReceptionStatus) : query.OrderBy(x => x.ReceptionStatus),
                "paymentstatus" => descending ? query.OrderByDescending(x => x.PaymentStatus) : query.OrderBy(x => x.PaymentStatus),
                _ => descending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt)
            };
        }
    }
}