using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceRule;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceRule;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handler_PriceRule
{
    public class CreatePriceListRuleHandler : IRequestHandler<CreatePriceListRuleCommand, PriceListRuleResponseDTO>
    {
        private readonly IGeneralRepository<PriceListRule> _ruleRepository;
        private readonly IGeneralRepository<PriceList> _priceListRepository;
        private readonly IMapper _mapper;

        public CreatePriceListRuleHandler(
            IGeneralRepository<PriceListRule> ruleRepository,
            IGeneralRepository<PriceList> priceListRepository,
            IMapper mapper)
        {
            _ruleRepository = ruleRepository;
            _priceListRepository = priceListRepository;
            _mapper = mapper;
        }

        public async Task<PriceListRuleResponseDTO> Handle(CreatePriceListRuleCommand request, CancellationToken cancellationToken)
        {
            // Validate Price List exists
            var priceListExists = await _priceListRepository.AnyAsync(pl => pl.Id == request.PriceListId);
            if (!priceListExists)
            {
                throw new NotFoundException(
                    $"Price list with ID {request.PriceListId} not found",
                    FinanceErrorCode.NotFound);
            }

            // Map DTO to Entity
            var rule = _mapper.Map<PriceListRule>(request.Data);
            rule.PriceListId = request.PriceListId;
            rule.CreatedAt = DateTime.UtcNow;

            // Add to repository
            await _ruleRepository.AddAsync(rule);
            await _ruleRepository.SaveChanges();

            // Return mapped response
            return _mapper.Map<PriceListRuleResponseDTO>(rule);
        }
    }
}
