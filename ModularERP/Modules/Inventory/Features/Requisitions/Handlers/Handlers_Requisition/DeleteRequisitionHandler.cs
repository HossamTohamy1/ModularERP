using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_Requisition;
using ModularERP.Modules.Inventory.Features.Requisitions.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Handlers.Handlers_Requisition
{
    public class DeleteRequisitionHandler : IRequestHandler<DeleteRequisitionCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<Requisition> _requisitionRepo;
        private readonly IFileUploadService _fileUploadService;
        private readonly FinanceDbContext _dbContext;

        public DeleteRequisitionHandler(
            IGeneralRepository<Requisition> requisitionRepo,
            IFileUploadService fileUploadService,
            FinanceDbContext dbContext)
        {
            _requisitionRepo = requisitionRepo;
            _fileUploadService = fileUploadService;
            _dbContext = dbContext;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            DeleteRequisitionCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var requisition = await _dbContext.Set<Requisition>()
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
                        "Only Draft requisitions can be deleted.",
                        "Inventory"
                    );
                }

                // Delete all attachments
                foreach (var attachment in requisition.Attachments)
                {
                    await _fileUploadService.DeleteFileAsync(attachment.FilePath);
                }

                // Delete requisition (cascade will delete items and attachments)
                await _requisitionRepo.Delete(requisition.Id);
                await _requisitionRepo.SaveChanges();

                return ResponseViewModel<bool>.Success(
                    true,
                    "Requisition deleted successfully."
                );
            }
            catch (Exception ex) when (ex is not BaseApplicationException)
            {
                throw new BusinessLogicException(
                    $"Error deleting requisition: {ex.Message}",
                    ex,
                    "Inventory"
                );
            }
        }
    }
}