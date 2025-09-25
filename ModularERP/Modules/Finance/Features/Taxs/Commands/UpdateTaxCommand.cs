using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Taxs.DTO;

namespace ModularERP.Modules.Finance.Features.Taxs.Commands
{
    public class UpdateTaxCommand : IRequest<ResponseViewModel<TaxResponseDto>>
    {
        public UpdateTaxDto UpdateTaxDto { get; set; }

        public UpdateTaxCommand(UpdateTaxDto updateTaxDto)
        {
            UpdateTaxDto = updateTaxDto;
        }
    }
}
