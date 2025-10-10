using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_BulkDiscount;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_BulkDiscount;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handlers_BulkDiscount
{
    public class UpdateBulkDiscountCommandHandler : IRequestHandler<UpdateBulkDiscountCommand, ResponseViewModel<BulkDiscountDto>>
    {
        private readonly IGeneralRepository<BulkDiscount> _repository;
        private readonly IGeneralRepository<PriceList> _priceListRepository;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IMapper _mapper;

        public UpdateBulkDiscountCommandHandler(
            IGeneralRepository<BulkDiscount> repository,
            IGeneralRepository<PriceList> priceListRepository,
            IGeneralRepository<Product> productRepository,
            IMapper mapper)
        {
            _repository = repository;
            _priceListRepository = priceListRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<BulkDiscountDto>> Handle(UpdateBulkDiscountCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByID(request.Id);
            if (entity == null || entity.PriceListId != request.PriceListId)
            {
                throw new NotFoundException(
                    $"Bulk discount with ID {request.Id} not found in price list {request.PriceListId}.",
                    FinanceErrorCode.NotFound);
            }

            // Validate Product exists
            var productExists = await _productRepository.AnyAsync(p => p.Id == request.ProductId && !p.IsDeleted);
            if (!productExists)
            {
                throw new NotFoundException(
                    $"Product with ID {request.ProductId} not found.",
                    FinanceErrorCode.NotFound);
            }

            // Check for overlapping discount ranges (excluding current record)
            var hasOverlap = await _repository.AnyAsync(bd =>
                bd.Id != request.Id &&
                bd.PriceListId == request.PriceListId &&
                bd.ProductId == request.ProductId &&
                !bd.IsDeleted &&
                ((request.MinQty >= bd.MinQty && request.MinQty <= (bd.MaxQty ?? decimal.MaxValue)) ||
                 (request.MaxQty.HasValue && request.MaxQty.Value >= bd.MinQty && request.MaxQty.Value <= (bd.MaxQty ?? decimal.MaxValue))));

            if (hasOverlap)
            {
                throw new BusinessLogicException(
                    "Discount range overlaps with existing bulk discount for this product.",
                    "Inventory",
                    FinanceErrorCode.BusinessLogicError);
            }

            _mapper.Map(request, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            await _repository.Update(entity);

            // استخدام Projection مع AutoMapper
            var dto = await _repository
                .GetAll()
                .Where(bd => bd.Id == entity.Id)
                .ProjectTo<BulkDiscountDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            return ResponseViewModel<BulkDiscountDto>.Success(dto, "Bulk discount updated successfully");
        }
    }
}