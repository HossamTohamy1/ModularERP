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
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Handlers.Handlers_Requisition
{
    public class CreateRequisitionHandler : IRequestHandler<CreateRequisitionCommand, ResponseViewModel<RequisitionResponseDto>>
    {
        private readonly IGeneralRepository<Requisition> _requisitionRepo;
        private readonly IGeneralRepository<Product> _productRepo;
        private readonly IGeneralRepository<WarehouseStock> _warehouseStockRepo;
        private readonly IFileUploadService _fileUploadService;
        private readonly IMapper _mapper;
        private readonly FinanceDbContext _dbContext;

        private readonly string[] _allowedExtensions =
        {
            ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx",
            ".xls", ".xlsx", ".txt", ".zip", ".rar"
        };
        private const int MaxFileSizeInMB = 10;

        public CreateRequisitionHandler(
            IGeneralRepository<Requisition> requisitionRepo,
            IGeneralRepository<Product> productRepo,
            IGeneralRepository<WarehouseStock> warehouseStockRepo,
            IFileUploadService fileUploadService,
            IMapper mapper,
            FinanceDbContext dbContext)
        {
            _requisitionRepo = requisitionRepo;
            _productRepo = productRepo;
            _warehouseStockRepo = warehouseStockRepo;
            _fileUploadService = fileUploadService;
            _mapper = mapper;
            _dbContext = dbContext;
        }

        public async Task<ResponseViewModel<RequisitionResponseDto>> Handle(
            CreateRequisitionCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                Guid userId = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53");

                // Validate attachments
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

                // Generate requisition number
                var requisitionNumber = await GenerateRequisitionNumber(request.CompanyId);

                // Create requisition entity
                var requisition = _mapper.Map<Requisition>(request);
                requisition.Number = requisitionNumber;
                requisition.Status = Common.Enum.Inventory_Enum.RequisitionStatus.Draft;
                requisition.CreatedById = userId;
                requisition.CreatedAt = DateTime.UtcNow;

                // Process items
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

                    // Get stock from warehouse
                    var warehouseStock = await _dbContext.Set<WarehouseStock>()
                        .Where(ws => ws.WarehouseId == request.WarehouseId
                                  && ws.ProductId == itemDto.ProductId)
                        .FirstOrDefaultAsync(cancellationToken);

                    decimal stockOnHand = warehouseStock?.AvailableQuantity ?? 0;

                    var item = _mapper.Map<RequisitionItem>(itemDto);
                    item.CreatedById = userId;
                    item.CreatedAt = DateTime.UtcNow;
                    item.StockOnHand = stockOnHand;
                    item.NewStockOnHand = request.Type == Common.Enum.Inventory_Enum.RequisitionType.Inbound
                        ? stockOnHand + item.Quantity
                        : stockOnHand - item.Quantity;
                    item.LineTotal = (item.UnitPrice ?? 0) * item.Quantity;

                    requisition.Items.Add(item);
                }

                // Process file attachments
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

                // Save requisition
                await _requisitionRepo.AddAsync(requisition);
                await _requisitionRepo.SaveChanges();

                // Retrieve with proper includes and projection
                var responseDto = await _dbContext.Set<Requisition>()
                    .Where(r => r.Id == requisition.Id)
                    .ProjectTo<RequisitionResponseDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);


                if (responseDto == null)
                {
                    throw new NotFoundException(
                        "Failed to retrieve created requisition.",
                        FinanceErrorCode.NotFound
                    );
                }

                return ResponseViewModel<RequisitionResponseDto>.Success(
                    responseDto,
                    "Requisition created successfully."
                );
            }
            catch (Exception ex) when (ex is not BaseApplicationException)
            {
                throw new BusinessLogicException(
                    $"Error creating requisition: {ex.Message}",
                    ex,
                    "Inventory"
                );
            }
        }

        private async Task<string> GenerateRequisitionNumber(Guid companyId)
        {
            var lastRequisition = await _requisitionRepo
                .GetByCompanyId(companyId)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            var lastNumber = lastRequisition?.Number ?? "REQ-0000";
            var numberPart = int.Parse(lastNumber.Split('-')[1]) + 1;

            return $"REQ-{numberPart:D4}";
        }
    }
}