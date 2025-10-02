using MediatR;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.TaxManagement.DTO;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Commends
{
    public class UpdateTaxComponentCommand : IRequest<ResponseViewModel<TaxComponentDto>>
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public TaxRateType RateType { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal RateValue { get; set; }

        [Required]
        public TaxIncludedType IncludedType { get; set; }

        public TaxAppliesOn AppliesOn { get; set; }

        public bool Active { get; set; }
    }
}
