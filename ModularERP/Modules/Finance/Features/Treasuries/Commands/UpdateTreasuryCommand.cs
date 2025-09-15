using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Treasuries.DTO;

namespace ModularERP.Modules.Finance.Features.Treasuries.Commands
{
    public class UpdateTreasuryCommand : IRequest<ResponseViewModel<bool>>
    {
        public UpdateTreasuryDto Treasury { get; set; } = new();
    }
}