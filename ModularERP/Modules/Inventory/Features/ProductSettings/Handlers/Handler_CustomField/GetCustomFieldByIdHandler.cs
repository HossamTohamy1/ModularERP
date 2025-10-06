using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Custom;
using ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries.Queries_CutomField;
using ModularERP.Modules.Inventory.Features.ProductSettings.Service;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handler_CustomField
{
    public class GetCustomFieldByIdHandler
    {
        private readonly ICustomFieldService _service;

        public GetCustomFieldByIdHandler(ICustomFieldService service)
        {
            _service = service;
        }

        public async Task<ResponseViewModel<CustomFieldResponseDto>> Handle(GetCustomFieldByIdQuery query)
        {
            return await _service.GetByIdAsync(query);
        }
    }
}
