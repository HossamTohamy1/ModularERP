using ModularERP.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Models
{
    public class BarcodeSettings : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string BarcodeType { get; set; } = "Code128"; // Code128, EAN13, UPC, QRCode, etc.

        public bool EnableWeightEmbedded { get; set; } = false;

        [MaxLength(50)]
        public string? EmbeddedBarcodeFormat { get; set; } // e.g., "XXXXXXXXWWWWWWPPPPN"

        public decimal? WeightUnitDivider { get; set; } // To convert embedded weight to system units

        public decimal? CurrencyDivider { get; set; } // To parse price from barcode

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Single settings per tenant (only one active record should exist)
        public bool IsDefault { get; set; } = true;
    }
}
