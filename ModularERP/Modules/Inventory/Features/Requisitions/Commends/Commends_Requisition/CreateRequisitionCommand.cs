using MediatR;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;
using System.Text.Json.Serialization;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_Requisition
{
    public class CreateRequisitionCommand : IRequest<ResponseViewModel<RequisitionResponseDto>>
    {
        public RequisitionType Type { get; set; }
        public DateTime Date { get; set; }
        public Guid WarehouseId { get; set; }
        public Guid? JournalAccountId { get; set; }
        public Guid? SupplierId { get; set; }
        public string? Notes { get; set; }
        public Guid CompanyId { get; set; }

        // يتم إرسال Items كـ JSON string في multipart form
        public string ItemsJson { get; set; } = string.Empty;

        [JsonIgnore]
        public List<CreateRequisitionItemDto> Items { get; set; } = new();

        // الملفات المرفقة
        public List<IFormFile>? Attachments { get; set; }
    }
}
