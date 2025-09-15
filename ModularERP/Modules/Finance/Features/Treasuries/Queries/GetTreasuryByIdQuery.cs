using MediatR;
using ModularERP.Common.ViewModel;
using System.ComponentModel.DataAnnotations;
using ModularERP.Modules.Finance.Features.Treasuries.DTO;

namespace ModularERP.Modules.Finance.Features.Treasuries.Queries
{
    public class GetTreasuryByIdQuery : IRequest<ResponseViewModel<TreasuryDto>>
    {
        [Required]
        public Guid Id { get; set; }
    }
}