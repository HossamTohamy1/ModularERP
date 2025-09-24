using FluentValidation;
using ModularERP.Modules.Finance.Features.GlAccounts.DTO;
using ModularERP.Modules.Finance.Features.GlAccounts.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.GlAccounts.Validators
{
    public class CreateGlAccountValidator : AbstractValidator<CreateGlAccountDto>
    {
        private readonly IGeneralRepository<GlAccount> _repository;

        public CreateGlAccountValidator(IGeneralRepository<GlAccount> repository)
        {
            _repository = repository;

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage("Account Code is required")
                .MaximumLength(20)
                .WithMessage("Account Code cannot exceed 20 characters")
                .MustAsync(BeUniqueCode)
                .WithMessage("Account Code must be unique within the company");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Account Name is required")
                .MaximumLength(100)
                .WithMessage("Account Name cannot exceed 100 characters");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid Account Type");

            RuleFor(x => x.CompanyId)
                .NotEmpty()
                .WithMessage("Company ID is required");
        }

        private async Task<bool> BeUniqueCode(CreateGlAccountDto dto, string code, CancellationToken cancellationToken)
        {
            return !await _repository.AnyAsync(x => x.Code == code && x.CompanyId == dto.CompanyId);
        }
    }
}
