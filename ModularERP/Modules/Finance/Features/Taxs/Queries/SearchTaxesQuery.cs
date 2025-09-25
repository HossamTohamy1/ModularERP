using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Taxs.DTO;

namespace ModularERP.Modules.Finance.Features.Taxs.Queries
{
    public class SearchTaxesQuery : IRequest<ResponseViewModel<List<TaxListDto>>>
    {
        public string SearchTerm { get; set; }

        public SearchTaxesQuery(string searchTerm)
        {
            SearchTerm = searchTerm ?? string.Empty;
        }
    }
}
