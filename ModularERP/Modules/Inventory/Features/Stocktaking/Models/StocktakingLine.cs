using ModularERP.Common.Models;
using ModularERP.Modules.Inventory.Features.Products.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Models
{
    public class StocktakingLine : BaseEntity
    {
        public Guid StocktakingId { get; set; }

        public Guid ProductId { get; set; }

        public Guid? UnitId { get; set; }

        /// <summary>
        /// Actual counted quantity
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,3)")]
        public decimal PhysicalQty { get; set; }

        /// <summary>
        /// System quantity at start of stocktaking
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,3)")]
        public decimal SystemQtySnapshot { get; set; }

        /// <summary>
        /// System quantity at time of posting
        /// </summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal? SystemQtyAtPost { get; set; }

        /// <summary>
        /// Variance = Physical - System
        /// </summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal? VarianceQty { get; set; }

        /// <summary>
        /// Cost basis for variance calculation
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? ValuationCost { get; set; }

        /// <summary>
        /// Financial impact of variance
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? VarianceValue { get; set; }

        public string? Note { get; set; }

        [MaxLength(500)]
        public string? ImagePath { get; set; }

        // Navigation Properties
        public virtual StocktakingHeader Stocktaking { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
