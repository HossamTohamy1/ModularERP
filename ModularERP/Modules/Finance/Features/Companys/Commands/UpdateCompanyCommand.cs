using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Companys.DTO;

namespace ModularERP.Modules.Finance.Features.Companys.Commands
{
    public class UpdateCompanyCommand : IRequest<ResponseViewModel<CompanyResponseDto>>
    {
        public UpdateCompanyDto CompanyDto { get; set; }

        public UpdateCompanyCommand(UpdateCompanyDto companyDto)
        {
            CompanyDto = companyDto;
        }
    }
}
