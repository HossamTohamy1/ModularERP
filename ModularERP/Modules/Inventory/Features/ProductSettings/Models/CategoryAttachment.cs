using ModularERP.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Models
{
    public class CategoryAttachment : BaseEntity
    {
        [Required]
        public Guid CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? FileType { get; set; }

        public long FileSize { get; set; }

        public Guid? UploadedBy { get; set; }
        public virtual ApplicationUser? UploadedByUser { get; set; }
    }
}
