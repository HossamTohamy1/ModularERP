using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.BankAccounts.DTO;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Commands
{
    public class CreateBankAccountCommand : IRequest<ResponseViewModel<BankAccountCreatedDto>>
    {
        public CreateBankAccountDto BankAccount { get; set; }

        // Parameterless constructor for dependency injection/serialization
        public CreateBankAccountCommand()
        {
            BankAccount = new CreateBankAccountDto();
        }

        // Constructor with parameter
        public CreateBankAccountCommand(CreateBankAccountDto bankAccount)
        {
            BankAccount = bankAccount ?? throw new ArgumentNullException(nameof(bankAccount));
        }
    }
}
