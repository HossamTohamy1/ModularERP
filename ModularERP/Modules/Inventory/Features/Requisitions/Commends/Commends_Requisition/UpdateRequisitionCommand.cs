using MediatR;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;
using System.Text.Json.Serialization;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_Requisition
{
    public class UpdateRequisitionCommand : IRequest<ResponseViewModel<RequisitionResponseDto>>
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        public RequisitionType Type { get; set; }
        public DateTime Date { get; set; }
        public Guid WarehouseId { get; set; }
        public Guid? JournalAccountId { get; set; }
        public Guid? SupplierId { get; set; }
        public string? Notes { get; set; }
        public Guid CompanyId { get; set; }

        [JsonIgnore]
        public List<CreateRequisitionItemDto> Items { get; set; } = new();

        public string? ItemsJson { get; set; }

        [JsonIgnore]
        public List<IFormFile>? Attachments { get; set; }

        [JsonIgnore]
        public List<Guid>? AttachmentsToRemove { get; set; }
    }
}
