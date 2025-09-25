using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Taxs.DTO;
using ModularERP.Modules.Finance.Features.Taxs.Models;
using ModularERP.Modules.Finance.Features.Taxs.Queries;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.Taxs.Handlers
{
    public class GetTaxByIdQueryHandler : IRequestHandler<GetTaxByIdQuery, ResponseViewModel<TaxResponseDto>>
    {
        private readonly IGeneralRepository<Tax> _taxRepository;
        private readonly IMapper _mapper;

        public GetTaxByIdQueryHandler(IGeneralRepository<Tax> taxRepository, IMapper mapper)
        {
            _taxRepository = taxRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<TaxResponseDto>> Handle(GetTaxByIdQuery request, CancellationToken cancellationToken)
        {
            var tax = await _taxRepository.GetByID(request.TaxId);

            if (tax == null)
            {
                throw new NotFoundException(
                    $"Tax with ID '{request.TaxId}' not found",
                    FinanceErrorCode.NotFound);
            }

            var response = _mapper.Map<TaxResponseDto>(tax);
            return ResponseViewModel<TaxResponseDto>.Success(response);
        }
    }
}
