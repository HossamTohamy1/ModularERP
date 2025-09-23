using FluentValidation;
using ModularERP.Modules.Finance.Features.Companys.DTO;

namespace ModularERP.Modules.Finance.Features.Companys.Validators
{
    public class UpdateCompanyDtoValidator : AbstractValidator<UpdateCompanyDto>
    {
        public UpdateCompanyDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Company ID is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Company name is required")
                .MaximumLength(100).WithMessage("Company name cannot exceed 100 characters")
                .Matches(@"^[a-zA-Z0-9\s\-_&.]+$").WithMessage("Company name contains invalid characters");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty().WithMessage("Currency code is required")
                .Length(3).WithMessage("Currency code must be exactly 3 characters")
                .Matches(@"^[A-Z]{3}$").WithMessage("Currency code must be 3 uppercase letters");
        }
    }
}
