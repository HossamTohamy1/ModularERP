using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Companys.DTO;

namespace ModularERP.Modules.Finance.Features.Companys.Commands
{
    public class CreateCompanyCommand : IRequest<ResponseViewModel<CompanyResponseDto>>
    {
        public CreateCompanyDto CompanyDto { get; set; }

        public CreateCompanyCommand(CreateCompanyDto companyDto)
        {
            CompanyDto = companyDto;
        }
    }

}
