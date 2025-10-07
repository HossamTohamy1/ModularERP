//using ModularERP.Common.ViewModel;
//using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Custom;
//using ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries.Queries_CutomField;
//using ModularERP.Modules.Inventory.Features.ProductSettings.Service;

//namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handler_CustomField
//{
//    public class GetCustomFieldsByEntityHandler
//    {
//        private readonly ICustomFieldService _service;

//        public GetCustomFieldsByEntityHandler(ICustomFieldService service)
//        {
//            _service = service;
//        }

//        public async Task<ResponseViewModel<List<CustomFieldResponseDto>>> Handle(GetCustomFieldsByEntityQuery query)
//        {
//            return await _service.GetByEntityTypeAsync(query);
//        }
//    }
//}
