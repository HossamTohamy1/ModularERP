using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Warehouses.DTO.DTO_WarehouseStock
{
    public class UpdateWarehouseStockDto
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 0")]
        public decimal Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Reserved quantity must be greater than or equal to 0")]
        public decimal? ReservedQuantity { get; set; }

        public decimal? AverageUnitCost { get; set; }
        public decimal? MinStockLevel { get; set; }
        public decimal? MaxStockLevel { get; set; }
        public decimal? ReorderPoint { get; set; }
    }
}
