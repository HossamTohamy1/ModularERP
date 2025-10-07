using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceListItems;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handlers_PriceListItems
{
    public class ClonePriceListCommandHandler : IRequestHandler<ClonePriceListCommand, ResponseViewModel<PriceListDto>>
    {
        private readonly IGeneralRepository<PriceList> _repository;
        private readonly IGeneralRepository<PriceListItem> _itemRepository;
        private readonly IGeneralRepository<PriceListRule> _ruleRepository;
        private readonly IGeneralRepository<BulkDiscount> _bulkDiscountRepository;
        private readonly IGeneralRepository<PriceListAssignment> _assignmentRepository;
        private readonly IMapper _mapper;

        public ClonePriceListCommandHandler(
            IGeneralRepository<PriceList> repository,
            IGeneralRepository<PriceListItem> itemRepository,
            IGeneralRepository<PriceListRule> ruleRepository,
            IGeneralRepository<BulkDiscount> bulkDiscountRepository,
            IGeneralRepository<PriceListAssignment> assignmentRepository,
            IMapper mapper)
        {
            _repository = repository;
            _itemRepository = itemRepository;
            _ruleRepository = ruleRepository;
            _bulkDiscountRepository = bulkDiscountRepository;
            _assignmentRepository = assignmentRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<PriceListDto>> Handle(ClonePriceListCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var sourcePriceList = await _repository.GetByID(request.CloneData.SourcePriceListId);
                if (sourcePriceList == null)
                {
                    throw new NotFoundException(
                        $"Source price list with ID {request.CloneData.SourcePriceListId} not found",
                        FinanceErrorCode.NotFound
                    );
                }

                // Check if new name already exists
                var nameExists = await _repository.AnyAsync(x => x.Name == request.CloneData.NewName && x.CompanyId == sourcePriceList.CompanyId);
                if (nameExists)
                {
                    throw new BusinessLogicException(
                        $"Price list with name '{request.CloneData.NewName}' already exists",
                        "Inventory"
                    );
                }

                // Clone the price list
                var newPriceList = new PriceList
                {
                    Id = Guid.NewGuid(),
                    CompanyId = sourcePriceList.CompanyId,
                    Name = request.CloneData.NewName,
                    Type = sourcePriceList.Type,
                    CurrencyCode = sourcePriceList.CurrencyCode,
                    ValidFrom = sourcePriceList.ValidFrom,
                    ValidTo = sourcePriceList.ValidTo,
                    IsDefault = false,
                    Status = PriceListStatus.Active
                };

                await _repository.AddAsync(newPriceList);
                await _repository.SaveChanges();

                // Clone items if requested
                if (request.CloneData.CopyItems)
                {
                    var items = await _itemRepository.Get(x => x.PriceListId == sourcePriceList.Id).ToListAsync(cancellationToken);
                    var newItems = items.Select(item => new PriceListItem
                    {
                        Id = Guid.NewGuid(),
                        PriceListId = newPriceList.Id,
                        ProductId = item.ProductId,
                        ServiceId = item.ServiceId,
                        BasePrice = item.BasePrice,
                        ListPrice = item.ListPrice,
                        DiscountValue = item.DiscountValue,
                        DiscountType = item.DiscountType,
                        FinalPrice = item.FinalPrice,
                        TaxProfileId = item.TaxProfileId,
                        ValidFrom = item.ValidFrom,
                        ValidTo = item.ValidTo
                    }).ToList();

                    await _itemRepository.AddRangeAsync(newItems);
                }

                // Clone rules if requested
                if (request.CloneData.CopyRules)
                {
                    var rules = await _ruleRepository.Get(x => x.PriceListId == sourcePriceList.Id).ToListAsync(cancellationToken);
                    var newRules = rules.Select(rule => new PriceListRule
                    {
                        Id = Guid.NewGuid(),
                        PriceListId = newPriceList.Id,
                        RuleType = rule.RuleType,
                        Value = rule.Value,
                        StartDate = rule.StartDate,
                        EndDate = rule.EndDate,
                        Priority = rule.Priority
                    }).ToList();

                    await _ruleRepository.AddRangeAsync(newRules);
                }

                // Clone bulk discounts if requested
                if (request.CloneData.CopyBulkDiscounts)
                {
                    var bulkDiscounts = await _bulkDiscountRepository.Get(x => x.PriceListId == sourcePriceList.Id).ToListAsync(cancellationToken);
                    var newBulkDiscounts = bulkDiscounts.Select(bd => new BulkDiscount
                    {
                        Id = Guid.NewGuid(),
                        PriceListId = newPriceList.Id,
                        ProductId = bd.ProductId,
                        MinQty = bd.MinQty,
                        MaxQty = bd.MaxQty,
                        DiscountType = bd.DiscountType,
                        DiscountValue = bd.DiscountValue
                    }).ToList();

                    await _bulkDiscountRepository.AddRangeAsync(newBulkDiscounts);
                }

                // Clone assignments if requested
                if (request.CloneData.CopyAssignments)
                {
                    var assignments = await _assignmentRepository.Get(x => x.PriceListId == sourcePriceList.Id).ToListAsync(cancellationToken);
                    var newAssignments = assignments.Select(a => new PriceListAssignment
                    {
                        Id = Guid.NewGuid(),
                        PriceListId = newPriceList.Id,
                        EntityType = a.EntityType,
                        EntityId = a.EntityId
                    }).ToList();

                    await _assignmentRepository.AddRangeAsync(newAssignments);
                }

                await _repository.SaveChanges();

                var result = await _repository
                    .GetAll()
                    .Where(x => x.Id == newPriceList.Id)
                    .ProjectTo<PriceListDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                return ResponseViewModel<PriceListDto>.Success(result, "Price list cloned successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (BusinessLogicException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException(
                    $"Error cloning price list: {ex.Message}",
                    "Inventory"
                );
            }
        }
    }
}