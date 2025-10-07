using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Command_Custom;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Custom;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries.Queries_CutomField;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Service
{
    public class CustomFieldService : ICustomFieldService
    {
        private readonly IGeneralRepository<CustomField> _repository;
        private readonly IMapper _mapper;

        public CustomFieldService(
            IGeneralRepository<CustomField> repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<CustomFieldResponseDto>> CreateAsync(CreateCustomFieldCommand command)
        {
            var existingField = await _repository
                .Get(x => x.FieldName.ToLower() == command.FieldName.ToLower())
                .AnyAsync();

            if (existingField)
            {
                throw new BusinessLogicException(
                    $"Custom field with name '{command.FieldName}' already exists",
                    "Inventory",
                    FinanceErrorCode.DuplicateEntity);
            }

            var customField = _mapper.Map<CustomField>(command);
            customField.Id = Guid.NewGuid();
            customField.CreatedAt = DateTime.UtcNow;
            customField.IsActive = true;

            await _repository.AddAsync(customField);
            await _repository.SaveChanges();

            var response = _mapper.Map<CustomFieldResponseDto>(customField);
            return ResponseViewModel<CustomFieldResponseDto>.Success(response, "Custom field created successfully");
        }

        public async Task<ResponseViewModel<CustomFieldResponseDto>> UpdateAsync(UpdateCustomFieldCommand command)
        {
            var existingField = await _repository.GetByID(command.Id);

            if (existingField == null)
            {
                throw new NotFoundException(
                    $"Custom field with ID '{command.Id}' not found",
                    FinanceErrorCode.NotFound);
            }

            var duplicateField = await _repository
                .Get(x => x.FieldName.ToLower() == command.FieldName.ToLower() && x.Id != command.Id)
                .AnyAsync();

            if (duplicateField)
            {
                throw new BusinessLogicException(
                    $"Custom field with name '{command.FieldName}' already exists",
                    "Inventory",
                    FinanceErrorCode.DuplicateEntity);
            }

            _mapper.Map(command, existingField);
            existingField.UpdatedAt = DateTime.UtcNow;

            await _repository.Update(existingField);

            var response = _mapper.Map<CustomFieldResponseDto>(existingField);
            return ResponseViewModel<CustomFieldResponseDto>.Success(response, "Custom field updated successfully");
        }

        public async Task<ResponseViewModel<bool>> DeleteAsync(DeleteCustomFieldCommand command)
        {
            var existingField = await _repository.GetByID(command.Id);

            if (existingField == null)
            {
                throw new NotFoundException(
                    $"Custom field with ID '{command.Id}' not found",
                    FinanceErrorCode.NotFound);
            }

            await _repository.Delete(command.Id);

            return ResponseViewModel<bool>.Success(true, "Custom field deleted successfully");
        }

        public async Task<ResponseViewModel<List<CustomFieldResponseDto>>> GetAllAsync(GetAllCustomFieldsQuery query)
        {
            var customFields = await _repository
                .GetAll()
                .OrderBy(x => x.DisplayOrder)
                .ProjectTo<CustomFieldResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return ResponseViewModel<List<CustomFieldResponseDto>>.Success(customFields, "Custom fields retrieved successfully");
        }

        public async Task<ResponseViewModel<CustomFieldResponseDto>> GetByIdAsync(GetCustomFieldByIdQuery query)
        {
            var customField = await _repository
                .Get(x => x.Id == query.Id)
                .ProjectTo<CustomFieldResponseDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (customField == null)
            {
                throw new NotFoundException(
                    $"Custom field with ID '{query.Id}' not found",
                    FinanceErrorCode.NotFound);
            }

            return ResponseViewModel<CustomFieldResponseDto>.Success(customField, "Custom field retrieved successfully");
        }

        //public async Task<ResponseViewModel<List<CustomFieldResponseDto>>> GetByEntityTypeAsync(GetCustomFieldsByEntityQuery query)
        //{
        //    var entityType = query.EntityType.ToLower();

        //    if (entityType != "product" && entityType != "service" && entityType != "warehouse")
        //    {
        //        throw new BusinessLogicException(
        //            "Invalid entity type. Allowed values are: product, service, warehouse",
        //            "Inventory",
        //            FinanceErrorCode.ValidationError);
        //    }

        //    var customFields = await _repository
        //        .GetAll()
        //        .Where(x => x.EntityType.ToLower() == entityType &&
        //                    x.Status == Common.Enum.Inventory_Enum.CustomFieldStatus.Active)
        //        .OrderBy(x => x.DisplayOrder)
        //        .ProjectTo<CustomFieldResponseDto>(_mapper.ConfigurationProvider)
        //        .ToListAsync();


        //    return ResponseViewModel<List<CustomFieldResponseDto>>.Success(
        //        customFields,
        //        $"Custom fields for {entityType} retrieved successfully");
        //}
    }
}