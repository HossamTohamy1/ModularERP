using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceListAssignment;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListAssignment;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handlers_PriceListAssignment
{
    public class UpdatePriceListAssignmentHandler : IRequestHandler<UpdatePriceListAssignmentCommand, ResponseViewModel<PriceListAssignmentDto>>
    {
        private readonly IGeneralRepository<PriceListAssignment> _repository;
        private readonly IGeneralRepository<PriceList> _priceListRepository;
        private readonly IMapper _mapper;

        public UpdatePriceListAssignmentHandler(
            IGeneralRepository<PriceListAssignment> repository,
            IGeneralRepository<PriceList> priceListRepository,
            IMapper mapper)
        {
            _repository = repository;
            _priceListRepository = priceListRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<PriceListAssignmentDto>> Handle(
            UpdatePriceListAssignmentCommand request,
            CancellationToken ct)
        {
            var entity = await _repository.GetByIDWithTracking(request.Id);

            if (entity == null)
            {
                throw new NotFoundException(
                    "Price list assignment not found",
                    FinanceErrorCode.NotFound);
            }

            // Check if PriceList exists
            var priceListExists = await _priceListRepository
                .GetAll()
                .AnyAsync(x => x.Id == request.Data.PriceListId, ct);

            if (!priceListExists)
            {
                throw new NotFoundException(
                    "Price List not found",
                    FinanceErrorCode.NotFound);
            }

            // Check for duplicate assignment (excluding current record)
            var isDuplicate = await _repository
                .GetAll()
                .AnyAsync(x =>
                    x.Id != request.Id &&
                    x.EntityType == request.Data.EntityType &&
                    x.EntityId == request.Data.EntityId &&
                    x.PriceListId == request.Data.PriceListId,
                    ct);

            if (isDuplicate)
            {
                throw new BusinessLogicException(
                    "This entity is already assigned to the selected price list",
                    "Inventory",
                    FinanceErrorCode.DuplicateEntity);
            }

            _mapper.Map(request.Data, entity);

            await _repository.Update(entity);

            var result = await _repository
                .GetAll()
                .Where(x => x.Id == request.Id)
                .ProjectTo<PriceListAssignmentDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(ct);

            return ResponseViewModel<PriceListAssignmentDto>.Success(
                result!,
                "Price list assignment updated successfully");
        }
    }
}