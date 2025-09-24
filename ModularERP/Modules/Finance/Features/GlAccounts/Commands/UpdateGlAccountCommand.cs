using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.GlAccounts.DTO;

namespace ModularERP.Modules.Finance.Features.GlAccounts.Commands
{
    public class UpdateGlAccountCommand : IRequest<ResponseViewModel<GlAccountResponseDto>>
    {
        public UpdateGlAccountDto GlAccount { get; set; }

        public UpdateGlAccountCommand(UpdateGlAccountDto glAccount)
        {
            GlAccount = glAccount;
        }
    }
}
