using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceListItems;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handlers_PriceListItems
{
    public class UpdatePriceListCommandHandler : IRequestHandler<UpdatePriceListCommand, ResponseViewModel<PriceListDto>>
    {
        private readonly IGeneralRepository<PriceList> _repository;
        private readonly IMapper _mapper;

        public UpdatePriceListCommandHandler(IGeneralRepository<PriceList> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<PriceListDto>> Handle(UpdatePriceListCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingPriceList = await _repository.GetByID(request.PriceList.Id);
                if (existingPriceList == null)
                {
                    throw new NotFoundException(
                        $"Price list with ID {request.PriceList.Id} not found",
                        FinanceErrorCode.NotFound
                    );
                }

                // Check name uniqueness
                var duplicateName = await _repository.AnyAsync(x =>
                    x.Name == request.PriceList.Name &&
                    x.CompanyId == request.PriceList.CompanyId &&
                    x.Id != request.PriceList.Id);

                if (duplicateName)
                {
                    throw new BusinessLogicException(
                        $"Price list with name '{request.PriceList.Name}' already exists for this company",
                        "Inventory"
                    );
                }

                // Handle IsDefault logic
                if (request.PriceList.IsDefault && !existingPriceList.IsDefault)
                {
                    var currentDefault = await _repository
                        .Get(x => x.CompanyId == request.PriceList.CompanyId &&
                                 x.Type == request.PriceList.Type &&
                                 x.IsDefault &&
                                 x.Id != request.PriceList.Id)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (currentDefault != null)
                    {
                        currentDefault.IsDefault = false;
                        await _repository.Update(currentDefault);
                    }
                }

                _mapper.Map(request.PriceList, existingPriceList);
                existingPriceList.UpdatedAt = DateTime.UtcNow;

                await _repository.Update(existingPriceList);

                var result = await _repository
                    .GetAll()
                    .Where(x => x.Id == existingPriceList.Id)
                    .ProjectTo<PriceListDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                return ResponseViewModel<PriceListDto>.Success(result, "Price list updated successfully");
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
                    $"Error updating price list: {ex.Message}",
                    "Inventory"
                );
            }
        }
    }
}
