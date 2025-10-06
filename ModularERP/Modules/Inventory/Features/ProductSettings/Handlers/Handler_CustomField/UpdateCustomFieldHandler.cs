using FluentValidation;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Command_Custom;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Custom;
using ModularERP.Modules.Inventory.Features.ProductSettings.Service;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handler_CustomField
{
    public class UpdateCustomFieldHandler
    {
        private readonly ICustomFieldService _service;
        private readonly IValidator<UpdateCustomFieldCommand> _validator;

        public UpdateCustomFieldHandler(
            ICustomFieldService service,
            IValidator<UpdateCustomFieldCommand> validator)
        {
            _service = service;
            _validator = validator;
        }

        public async Task<ResponseViewModel<CustomFieldResponseDto>> Handle(UpdateCustomFieldCommand command)
        {
            var validationResult = await _validator.ValidateAsync(command);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                throw new ModularERP.Common.Exceptions.ValidationException(
                    "Validation failed for custom field update",
                    errors,
                    "Inventory");
            }

            return await _service.UpdateAsync(command);
        }
    }
}

