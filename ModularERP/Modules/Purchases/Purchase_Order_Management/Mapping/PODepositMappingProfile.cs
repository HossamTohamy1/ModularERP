using AutoMapper;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PODeposite;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PODeposite;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Mapping
{
    public class PODepositMappingProfile : Profile
    {
        public PODepositMappingProfile()
        {
            CreateMap<PODeposit, PODepositResponseDto>();

            CreateMap<CreatePODepositCommand, PODeposit>();

            CreateMap<UpdatePODepositCommand, PODeposit>();
        }
    }
}
