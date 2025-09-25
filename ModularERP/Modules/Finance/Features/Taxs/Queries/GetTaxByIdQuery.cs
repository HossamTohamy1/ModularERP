using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Taxs.DTO;

namespace ModularERP.Modules.Finance.Features.Taxs.Queries
{
    public class GetTaxByIdQuery : IRequest<ResponseViewModel<TaxResponseDto>>
    {
        public Guid TaxId { get; set; }

        public GetTaxByIdQuery(Guid taxId)
        {
            TaxId = taxId;
        }
    }
}
