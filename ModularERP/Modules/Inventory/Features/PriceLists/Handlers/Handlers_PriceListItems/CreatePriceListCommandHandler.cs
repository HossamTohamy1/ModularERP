using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceListItems;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handlers_PriceListItems
{
    public class CreatePriceListCommandHandler : IRequestHandler<CreatePriceListCommand, ResponseViewModel<PriceListDto>>
    {
        private readonly IGeneralRepository<PriceList> _repository;
        private readonly IMapper _mapper;

        public CreatePriceListCommandHandler(IGeneralRepository<PriceList> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<PriceListDto>> Handle(CreatePriceListCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if name already exists
                var exists = await _repository.AnyAsync(x => x.Name == request.PriceList.Name && x.CompanyId == request.PriceList.CompanyId);
                if (exists)
                {
                    throw new BusinessLogicException(
                        $"Price list with name '{request.PriceList.Name}' already exists for this company",
                        "Inventory"
                    );
                }

                // Handle IsDefault logic
                if (request.PriceList.IsDefault)
                {
                    var existingDefault = await _repository
                        .Get(x => x.CompanyId == request.PriceList.CompanyId &&
                                 x.Type == request.PriceList.Type &&
                                 x.IsDefault)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (existingDefault != null)
                    {
                        existingDefault.IsDefault = false;
                        await _repository.Update(existingDefault);
                    }
                }

                var priceList = _mapper.Map<PriceList>(request.PriceList);
                priceList.Id = Guid.NewGuid();

                await _repository.AddAsync(priceList);
                await _repository.SaveChanges();

                var result = await _repository
                    .GetAll()
                    .Where(x => x.Id == priceList.Id)
                    .ProjectTo<PriceListDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                return ResponseViewModel<PriceListDto>.Success(result, "Price list created successfully");
            }
            catch (BusinessLogicException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException(
                    $"Error creating price list: {ex.Message}",
                    "Inventory"
                );
            }
        }
    }
}
