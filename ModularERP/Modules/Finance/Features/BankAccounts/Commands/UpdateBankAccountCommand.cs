using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.BankAccounts.DTO;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Commands
{
    public class UpdateBankAccountCommand : IRequest<ResponseViewModel<bool>>
    {
        public UpdateBankAccountDto BankAccount { get; set; }

        public UpdateBankAccountCommand()
        {
            BankAccount = new UpdateBankAccountDto();
        }

        public UpdateBankAccountCommand(UpdateBankAccountDto bankAccount)
        {
            BankAccount = bankAccount ?? throw new ArgumentNullException(nameof(bankAccount));
        }
    }
}
