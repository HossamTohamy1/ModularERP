using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_Requisition;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;
using ModularERP.Modules.Inventory.Features.Requisitions.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Handlers.Handlers_Requisition
{
    public class UpdateRequisitionHandler : IRequestHandler<UpdateRequisitionCommand, ResponseViewModel<RequisitionResponseDto>>
    {
        private readonly IGeneralRepository<Requisition> _requisitionRepo;
        private readonly IGeneralRepository<Product> _productRepo;
        private readonly IFileUploadService _fileUploadService;
        private readonly IMapper _mapper;
        private readonly FinanceDbContext _dbContext;

        private readonly string[] _allowedExtensions =
        {
            ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx",
            ".xls", ".xlsx", ".txt", ".zip", ".rar"
        };
        private const int MaxFileSizeInMB = 10;

        public UpdateRequisitionHandler(
            IGeneralRepository<Requisition> requisitionRepo,
            IGeneralRepository<Product> productRepo,
            IFileUploadService fileUploadService,
            IMapper mapper,
            FinanceDbContext dbContext)
        {
            _requisitionRepo = requisitionRepo;
            _productRepo = productRepo;
            _fileUploadService = fileUploadService;
            _mapper = mapper;
            _dbContext = dbContext;
        }

        public async Task<ResponseViewModel<RequisitionResponseDto>> Handle(
            UpdateRequisitionCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                Guid userId = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53");

                // Get existing requisition
                var requisition = await _dbContext.Set<Requisition>()
                    .Include(r => r.Items)
                    .Include(r => r.Attachments)
                    .FirstOrDefaultAsync(r => r.Id == request.Id && r.CompanyId == request.CompanyId, cancellationToken);

                if (requisition == null)
                {
                    throw new NotFoundException(
                        $"Requisition with ID {request.Id} not found.",
                        FinanceErrorCode.NotFound
                    );
                }

                if (requisition.Status != RequisitionStatus.Draft)
                {
                    throw new BusinessLogicException(
                        "Only Draft requisitions can be edited.",
                        "Inventory"
                    );
                }

                // Validate attachments if provided
                if (request.Attachments != null && request.Attachments.Any())
                {
                    foreach (var file in request.Attachments)
                    {
                        var (isValid, errorMessage) = _fileUploadService.ValidateFile(
                            file,
                            _allowedExtensions,
                            MaxFileSizeInMB
                        );

                        if (!isValid)
                        {
                            throw new BusinessLogicException(
                                $"File validation failed for '{file.FileName}': {errorMessage}",
                                "Inventory"
                            );
                        }
                    }
                }

                // Update basic properties
                requisition.Type = request.Type;
                requisition.Date = request.Date;
                requisition.WarehouseId = request.WarehouseId;
                requisition.JournalAccountId = request.JournalAccountId;
                requisition.SupplierId = request.SupplierId;
                requisition.Notes = request.Notes;
                requisition.UpdatedAt = DateTime.UtcNow;
                requisition.UpdatedById = userId;

                // Update items - remove old items
                _dbContext.Set<RequisitionItem>().RemoveRange(requisition.Items);

                // Add new items
                foreach (var itemDto in request.Items)
                {
                    var product = await _productRepo.GetByID(itemDto.ProductId);
                    if (product == null)
                    {
                        throw new NotFoundException(
                            $"Product with ID {itemDto.ProductId} not found.",
                            FinanceErrorCode.NotFound
                        );
                    }

                    var item = _mapper.Map<RequisitionItem>(itemDto);
                    item.CreatedById = userId;
                    item.CreatedAt = DateTime.UtcNow;
                    item.StockOnHand = 0; // TODO: Get from warehouse
                    item.NewStockOnHand = request.Type == RequisitionType.Inbound
                        ? (item.StockOnHand ?? 0) + item.Quantity
                        : (item.StockOnHand ?? 0) - item.Quantity;
                    item.LineTotal = (item.UnitPrice ?? 0) * item.Quantity;

                    requisition.Items.Add(item);
                }

                // Handle attachment removals
                if (request.AttachmentsToRemove != null && request.AttachmentsToRemove.Any())
                {
                    var attachmentsToDelete = requisition.Attachments
                        .Where(a => request.AttachmentsToRemove.Contains(a.Id))
                        .ToList();

                    foreach (var attachment in attachmentsToDelete)
                    {
                        // Delete physical file
                        await _fileUploadService.DeleteFileAsync(attachment.FilePath);
                        _dbContext.Set<RequisitionAttachment>().Remove(attachment);
                    }
                }

                // Add new attachments
                if (request.Attachments != null && request.Attachments.Any())
                {
                    var folderPath = $"Requisitions/{DateTime.UtcNow:yyyy/MM}";
                    var uploadResults = await _fileUploadService.UploadFilesAsync(
                        request.Attachments,
                        folderPath
                    );

                    foreach (var uploadResult in uploadResults)
                    {
                        var attachment = new RequisitionAttachment
                        {
                            Filename = uploadResult.FileName,
                            FilePath = uploadResult.FilePath,
                            MimeType = uploadResult.MimeType,
                            FileSize = uploadResult.FileSize,
                            Checksum = uploadResult.Checksum,
                            UploadedBy = userId,
                            UploadedAt = DateTime.UtcNow,
                            CreatedById = userId,
                            CreatedAt = DateTime.UtcNow
                        };

                        requisition.Attachments.Add(attachment);
                    }
                }

                // Save changes
                await _dbContext.SaveChangesAsync(cancellationToken);

                // Retrieve updated requisition with projection
                var responseDto = await _dbContext.Set<Requisition>()
                    .Where(r => r.Id == requisition.Id)
                    .ProjectTo<RequisitionResponseDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                if (responseDto == null)
                {
                    throw new NotFoundException(
                        "Failed to retrieve updated requisition.",
                        FinanceErrorCode.NotFound
                    );
                }

                return ResponseViewModel<RequisitionResponseDto>.Success(
                    responseDto,
                    "Requisition updated successfully."
                );
            }
            catch (Exception ex) when (ex is not BaseApplicationException)
            {
                throw new BusinessLogicException(
                    $"Error updating requisition: {ex.Message}",
                    ex,
                    "Inventory"
                );
            }
        }
    }
}