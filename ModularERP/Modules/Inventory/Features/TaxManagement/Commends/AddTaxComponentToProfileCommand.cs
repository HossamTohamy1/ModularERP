using MediatR;
using ModularERP.Common.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Commends
{
    public class AddTaxComponentToProfileCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid TaxProfileId { get; set; }

        [Required]
        public Guid TaxComponentId { get; set; }

        [Range(1, 100)]
        public int Priority { get; set; } = 1;
    }
}
