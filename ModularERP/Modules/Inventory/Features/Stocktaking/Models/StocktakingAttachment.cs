using ModularERP.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Models
{
    public class StocktakingAttachment : BaseEntity
    {
        public Guid StocktakingId { get; set; }

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        public Guid? UploadedBy { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual StocktakingHeader Stocktaking { get; set; } = null!;
        public virtual ApplicationUser? UploadedByUser { get; set; }
    }
}
