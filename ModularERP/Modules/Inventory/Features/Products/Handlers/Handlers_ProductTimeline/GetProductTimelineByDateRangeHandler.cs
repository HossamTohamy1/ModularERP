using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ProductTimeline;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Modules.Inventory.Features.Products.Qeuries.Qeuries_Product;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Products.Handlers.Handlers_ProductTimeline
{
    public class GetProductTimelineByDateRangeHandler : IRequestHandler<GetProductTimelineByDateRangeQuery, ResponseViewModel<IEnumerable<ProductTimelineDto>>>
    {
        private readonly IGeneralRepository<ProductTimeline> _repository;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IMapper _mapper;

        public GetProductTimelineByDateRangeHandler(
            IGeneralRepository<ProductTimeline> repository,
            IGeneralRepository<Product> productRepository,
            IMapper mapper)
        {
            _repository = repository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<IEnumerable<ProductTimelineDto>>> Handle(
            GetProductTimelineByDateRangeQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var product = await _productRepository.GetByID(request.ProductId);
                if (product == null)
                    throw new NotFoundException($"Product with ID {request.ProductId} not found", FinanceErrorCode.NotFound);

                var items = _repository.Get(x =>
                        x.ProductId == request.ProductId &&
                        x.CreatedAt >= request.StartDate &&
                        x.CreatedAt <= request.EndDate)
                    .OrderByDescending(x => x.CreatedAt)
                    .AsEnumerable()
                    .Select(x => _mapper.Map<ProductTimelineDto>(x))
                    .ToList();

                return ResponseViewModel<IEnumerable<ProductTimelineDto>>.Success(items, "Timeline retrieved successfully");
            }
            catch (NotFoundException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException(
                    "Error retrieving product timeline by date range",
                    ex,
                    "Products",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }

}
