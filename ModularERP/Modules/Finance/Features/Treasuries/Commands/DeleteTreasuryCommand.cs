using MediatR;
using ModularERP.Common.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Finance.Features.Treasuries.Commands
{
    public class DeleteTreasuryCommand : IRequest<ResponseViewModel<bool>>
    {
        [Required]
        public Guid Id { get; set; }
    }
}