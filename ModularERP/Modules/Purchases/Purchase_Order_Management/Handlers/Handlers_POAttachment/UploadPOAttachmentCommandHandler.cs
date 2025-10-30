using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_POAttachment;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POAttachment;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_POAttachment
{
    public class UploadPOAttachmentCommandHandler : IRequestHandler<UploadPOAttachmentCommand, ResponseViewModel<POAttachmentResponseDto>>
    {
        private readonly IGeneralRepository<POAttachment> _attachmentRepository;
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UploadPOAttachmentCommandHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _uploadPath;

        // Allowed file types
        private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".gif" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public UploadPOAttachmentCommandHandler(
            IGeneralRepository<POAttachment> attachmentRepository,
            IGeneralRepository<PurchaseOrder> poRepository,
            IMapper mapper,
            ILogger<UploadPOAttachmentCommandHandler> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _attachmentRepository = attachmentRepository;
            _poRepository = poRepository;
            _mapper = mapper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;

            // Define upload path (adjust based on your configuration)
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "po-attachments");

            // Ensure directory exists
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<ResponseViewModel<POAttachmentResponseDto>> Handle(UploadPOAttachmentCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Uploading attachment for Purchase Order {PurchaseOrderId}", request.PurchaseOrderId);

            // Validate PO exists
            var poExists = await _poRepository.AnyAsync(po => po.Id == request.PurchaseOrderId && !po.IsDeleted, cancellationToken);
            if (!poExists)
            {
                throw new NotFoundException($"Purchase Order with ID {request.PurchaseOrderId} not found", FinanceErrorCode.NotFound);
            }

            // Validate file
            if (request.File == null || request.File.Length == 0)
            {
                throw new ValidationException("File is required", new Dictionary<string, string[]>
                {
                    { "File", new[] { "File cannot be empty" } }
                }, "Purchases");
            }

            // Check file size
            if (request.File.Length > MaxFileSize)
            {
                throw new ValidationException("File size exceeds limit", new Dictionary<string, string[]>
                {
                    { "File", new[] { $"File size must not exceed {MaxFileSize / (1024 * 1024)}MB" } }
                }, "Purchases");
            }

            // Check file extension
            var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                throw new ValidationException("Invalid file type", new Dictionary<string, string[]>
                {
                    { "File", new[] { $"Only the following file types are allowed: {string.Join(", ", _allowedExtensions)}" } }
                }, "Purchases");
            }

            // Generate unique file name
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(request.File.FileName)}";
            var filePath = Path.Combine(_uploadPath, uniqueFileName);

            // Save file to disk
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file to disk");
                throw new BusinessLogicException("Failed to save file", "Purchases");
            }

            // Get current user ID
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
                         ?? _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;

            var uploadedBy = string.IsNullOrEmpty(userId) ? Guid.Empty : Guid.Parse(userId);

            // Create attachment record
            var attachment = new POAttachment
            {
                Id = Guid.NewGuid(),
                PurchaseOrderId = request.PurchaseOrderId,
                FileName = request.File.FileName,
                FilePath = $"/uploads/po-attachments/{uniqueFileName}",
                FileType = extension,
                FileSize = request.File.Length,
                UploadedAt = DateTime.UtcNow,
                UploadedBy = uploadedBy,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _attachmentRepository.AddAsync(attachment);
            await _attachmentRepository.SaveChanges();

            _logger.LogInformation("Attachment {AttachmentId} uploaded successfully for PO {PurchaseOrderId}", attachment.Id, request.PurchaseOrderId);

            var response = _mapper.Map<POAttachmentResponseDto>(attachment);
            return ResponseViewModel<POAttachmentResponseDto>.Success(response, "Attachment uploaded successfully");
        }
    }
}
