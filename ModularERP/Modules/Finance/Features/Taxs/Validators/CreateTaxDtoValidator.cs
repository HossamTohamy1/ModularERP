using FluentValidation;
using ModularERP.Modules.Finance.Features.Taxs.DTO;

namespace ModularERP.Modules.Finance.Features.Taxs.Validators
{
    public class CreateTaxDtoValidator : AbstractValidator<CreateTaxDto>
    {
        public CreateTaxDtoValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage("Tax code is required")
                .MaximumLength(20)
                .WithMessage("Tax code cannot exceed 20 characters")
                .Matches(@"^[A-Za-z0-9_-]+$")
                .WithMessage("Tax code can only contain letters, numbers, underscores, and hyphens");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tax name is required")
                .MaximumLength(100)
                .WithMessage("Tax name cannot exceed 100 characters")
                .MinimumLength(3)
                .WithMessage("Tax name must be at least 3 characters long");

            RuleFor(x => x.Rate)
                .GreaterThanOrEqualTo(0).WithMessage("Tax rate cannot be negative")
                .LessThanOrEqualTo(100).WithMessage("Tax rate cannot exceed 100%")
                .Must(rate => DecimalPlaces(rate) <= 2)
                .WithMessage("Tax rate cannot have more than 2 decimal places");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid tax type specified");
        }
        private int DecimalPlaces(decimal number)
        {
            number = Math.Abs(number); // للتأكد من القيمة الموجبة
            int decimalPlaces = BitConverter.GetBytes(decimal.GetBits(number)[3])[2];
            return decimalPlaces;
        }
    }
}
