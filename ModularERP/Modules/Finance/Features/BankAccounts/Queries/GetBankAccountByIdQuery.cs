using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.BankAccounts.DTO;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Queries
{
    public class GetBankAccountByIdQuery : IRequest<ResponseViewModel<BankAccountDto>>
    {
        public Guid Id { get; set; }

        public GetBankAccountByIdQuery()
        {
            Id = Guid.Empty;
        }

        public GetBankAccountByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
