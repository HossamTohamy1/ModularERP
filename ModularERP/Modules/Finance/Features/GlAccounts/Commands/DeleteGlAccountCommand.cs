using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Finance.Features.GlAccounts.Commands
{
    public class DeleteGlAccountCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }

        public DeleteGlAccountCommand(Guid id)
        {
            Id = id;
        }
    }
}
