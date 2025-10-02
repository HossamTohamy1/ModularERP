using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.TaxManagement.DTO;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Commends
{
    public class CreateTaxProfileCommand : IRequest<ResponseViewModel<TaxProfileDto>>
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool Active { get; set; } = true;

        public List<TaxProfileComponentRequest> Components { get; set; } = new();
    }
}
