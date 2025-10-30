using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POAttachment;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Qeuries.Qeuries_POAttachment;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_POAttachment
{
    public class GetPOAttachmentsQueryHandler : IRequestHandler<GetPOAttachmentsQuery, ResponseViewModel<List<POAttachmentResponseDto>>>
    {
        private readonly IGeneralRepository<POAttachment> _attachmentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPOAttachmentsQueryHandler> _logger;

        public GetPOAttachmentsQueryHandler(
            IGeneralRepository<POAttachment> attachmentRepository,
            IMapper mapper,
            ILogger<GetPOAttachmentsQueryHandler> logger)
        {
            _attachmentRepository = attachmentRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<POAttachmentResponseDto>>> Handle(GetPOAttachmentsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving attachments for Purchase Order {PurchaseOrderId}", request.PurchaseOrderId);

            var attachments = await _attachmentRepository
                .Get(a => a.PurchaseOrderId == request.PurchaseOrderId)
                .OrderByDescending(a => a.UploadedAt)
                .ProjectTo<POAttachmentResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Retrieved {Count} attachments for Purchase Order {PurchaseOrderId}", attachments.Count, request.PurchaseOrderId);

            return ResponseViewModel<List<POAttachmentResponseDto>>.Success(attachments, "Attachments retrieved successfully");
        }
    }
}
