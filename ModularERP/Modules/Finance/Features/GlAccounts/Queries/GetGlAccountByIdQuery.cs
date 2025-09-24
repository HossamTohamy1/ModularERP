using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.GlAccounts.DTO;

namespace ModularERP.Modules.Finance.Features.GlAccounts.Queries
{
    public class GetGlAccountByIdQuery : IRequest<ResponseViewModel<GlAccountResponseDto>>
    {
        public Guid Id { get; set; }

        public GetGlAccountByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
