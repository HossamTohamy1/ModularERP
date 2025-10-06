using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Command_Custom;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Custom;
using ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries.Queries_CutomField;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Service
{
    public interface ICustomFieldService
    {
        Task<ResponseViewModel<CustomFieldResponseDto>> CreateAsync(CreateCustomFieldCommand command);
        Task<ResponseViewModel<CustomFieldResponseDto>> UpdateAsync(UpdateCustomFieldCommand command);
        Task<ResponseViewModel<bool>> DeleteAsync(DeleteCustomFieldCommand command);
        Task<ResponseViewModel<List<CustomFieldResponseDto>>> GetAllAsync(GetAllCustomFieldsQuery query);
        Task<ResponseViewModel<CustomFieldResponseDto>> GetByIdAsync(GetCustomFieldByIdQuery query);
        Task<ResponseViewModel<List<CustomFieldResponseDto>>> GetByEntityTypeAsync(GetCustomFieldsByEntityQuery query);
    }
}
