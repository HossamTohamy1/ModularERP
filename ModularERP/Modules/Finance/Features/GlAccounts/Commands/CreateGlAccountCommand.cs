using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.GlAccounts.DTO;

namespace ModularERP.Modules.Finance.Features.GlAccounts.Commands
{
    public class CreateGlAccountCommand : IRequest<ResponseViewModel<GlAccountResponseDto>>
    {
        public CreateGlAccountDto GlAccount { get; set; }

        public CreateGlAccountCommand(CreateGlAccountDto glAccount)
        {
            GlAccount = glAccount;
        }
    }
}
