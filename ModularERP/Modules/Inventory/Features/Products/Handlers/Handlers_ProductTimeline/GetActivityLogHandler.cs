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
    public class GetActivityLogHandler : IRequestHandler<GetActivityLogQuery, ResponseViewModel<PaginatedResult<ActivityLogDto>>>
    {
        private readonly IGeneralRepository<ActivityLog> _repository;
        private readonly IMapper _mapper;

        public GetActivityLogHandler(IGeneralRepository<ActivityLog> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<PaginatedResult<ActivityLogDto>>> Handle(
            GetActivityLogQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = _repository.GetAll().OrderByDescending(x => x.CreatedAt);
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

                return ResponseViewModel<PaginatedResult<ActivityLogDto>>.Success(result, "Activity logs retrieved successfully");
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException(
                    "Error retrieving activity logs",
                    ex,
                    "Products",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }

}
