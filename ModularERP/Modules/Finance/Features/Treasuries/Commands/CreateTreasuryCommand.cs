using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Treasuries.DTO;

namespace ModularERP.Modules.Finance.Features.Treasuries.Commands
{
    public class CreateTreasuryCommand : IRequest<ResponseViewModel<TreasuryCreatedDto>>
    {
        public CreateTreasuryDto Treasury { get; set; } = new();
    }
}