using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Finance.Features.Companys.Commands
{
    public class DeleteCompanyCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }

        public DeleteCompanyCommand(Guid id)
        {
            Id = id;
        }
    }
}
