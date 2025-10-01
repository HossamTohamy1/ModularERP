using FluentValidation;
using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Brand;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Shared.Interfaces;
using AppValidationException = ModularERP.Common.Exceptions.ValidationException;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handelrs_Brand
{
    public class DeleteBrandHandler : IRequestHandler<DeleteBrandCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<Brand> _repository;
        private readonly IValidator<DeleteBrandCommand> _validator;

        public DeleteBrandHandler(
            IGeneralRepository<Brand> repository,
            IValidator<DeleteBrandCommand> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                throw new AppValidationException("Validation failed for deleting brand", errors, "Inventory");
            }

            await _repository.Delete(request.Id);
            return ResponseViewModel<bool>.Success(true, "Brand deleted successfully");
        }
    }
}
