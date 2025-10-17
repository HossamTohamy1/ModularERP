using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ProductTimeline;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Modules.Inventory.Features.Products.Qeuries.Qeuries_Product;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Category;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Products.Handlers.Handlers_ProductTimeline
{
    public class GetProductActivityLogHandler : IRequestHandler<GetProductActivityLogQuery, ResponseViewModel<PaginatedResult<ActivityLogDto>>>
    {
        private readonly IGeneralRepository<ActivityLog> _repository;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IMapper _mapper;

        public GetProductActivityLogHandler(
            IGeneralRepository<ActivityLog> repository,
            IGeneralRepository<Product> productRepository,
            IMapper mapper)
        {
            _repository = repository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<PaginatedResult<ActivityLogDto>>> Handle(
            GetProductActivityLogQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var product = await _productRepository.GetByID(request.ProductId);
                if (product == null)
                    throw new NotFoundException($"Product with ID {request.ProductId} not found", FinanceErrorCode.NotFound);

                var query = _repository.Get(x => x.ProductId == request.ProductId)
                    .OrderByDescending(x => x.CreatedAt);

                var totalCount = query.Count();
                var items = query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .AsEnumerable()
                    .Select(x => _mapper.Map<ActivityLogDto>(x))
                    .ToList();

                var result = new PaginatedResult<ActivityLogDto>
                {
                    Items = items,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalCount = totalCount
                };

                return ResponseViewModel<PaginatedResult<ActivityLogDto>>.Success(result, "Product activity logs retrieved successfully");
            }
            catch (NotFoundException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException(
                    "Error retrieving product activity logs",
                    ex,
                    "Products",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }
}
