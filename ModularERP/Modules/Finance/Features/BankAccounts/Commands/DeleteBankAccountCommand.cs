using MediatR;
using ModularERP.Common.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Commands
{
    public class DeleteBankAccountCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }

        public DeleteBankAccountCommand()
        {
            Id = Guid.Empty;
        }

        public DeleteBankAccountCommand(Guid id)
        {
            Id = id;
        }
    }
}
