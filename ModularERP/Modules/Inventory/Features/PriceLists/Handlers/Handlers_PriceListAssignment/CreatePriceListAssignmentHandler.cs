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
    public class CreatePriceListAssignmentHandler : IRequestHandler<CreatePriceListAssignmentCommand, ResponseViewModel<PriceListAssignmentDto>>
    {
        private readonly IGeneralRepository<PriceListAssignment> _repository;
        private readonly IGeneralRepository<PriceList> _priceListRepository;
        private readonly IMapper _mapper;

        public CreatePriceListAssignmentHandler(
            IGeneralRepository<PriceListAssignment> repository,
            IGeneralRepository<PriceList> priceListRepository,
            IMapper mapper)
        {
            _repository = repository;
            _priceListRepository = priceListRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<PriceListAssignmentDto>> Handle(
            CreatePriceListAssignmentCommand request,
            CancellationToken ct)
        {
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

            // Check for duplicate assignment
            var isDuplicate = await _repository
                .GetAll()
                .AnyAsync(x =>
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

            var entity = _mapper.Map<PriceListAssignment>(request.Data);

            await _repository.AddAsync(entity);
            await _repository.SaveChanges();

            var result = await _repository
                .GetAll()
                .Where(x => x.Id == entity.Id)
                .ProjectTo<PriceListAssignmentDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(ct);

            return ResponseViewModel<PriceListAssignmentDto>.Success(
                result!,
                "Price list assignment created successfully");
        }
    }
}