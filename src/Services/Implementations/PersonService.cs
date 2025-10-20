using BadApiExample.DTOs;
using BadApiExample.Models;
using BadApiExample.Data.Repositories;
using BadApiExample.Data;
using Microsoft.Extensions.Logging;

namespace BadApiExample.Services.Implementations;

public class PersonService : IPersonService
{
    private readonly IPersonRepository _personRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PersonService> _logger;

    public PersonService(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        ILogger<PersonService> logger)
    {
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponseDto<PersonResponseDto>> GetByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                return ApiResponseDto<PersonResponseDto>.ErrorResult("Invalid ID provided");
            }

            var person = await _personRepository.GetByIdAsync(id);
            if (person == null)
            {
                return ApiResponseDto<PersonResponseDto>.ErrorResult("Person not found");
            }

            var responseDto = MapToResponseDto(person);
            return ApiResponseDto<PersonResponseDto>.SuccessResult(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving person with ID {PersonId}", id);
            return ApiResponseDto<PersonResponseDto>.ErrorResult("An error occurred while retrieving the person");
        }
    }

    public async Task<ApiResponseDto<PagedResultDto<PersonResponseDto>>> GetAllAsync(PagedRequestDto request)
    {
        try
        {
            var (items, totalCount) = await _personRepository.GetPagedAsync(
                request.Page,
                request.PageSize,
                filter: null,
                orderBy: q => q.OrderBy(p => p.Id));

            var responseDtos = items.Select(MapToResponseDto);

            var pagedResult = new PagedResultDto<PersonResponseDto>
            {
                Data = responseDtos,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };

            return ApiResponseDto<PagedResultDto<PersonResponseDto>>.SuccessResult(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving persons with pagination");
            return ApiResponseDto<PagedResultDto<PersonResponseDto>>.ErrorResult("An error occurred while retrieving persons");
        }
    }

    public async Task<ApiResponseDto<PagedResultDto<PersonResponseDto>>> SearchAsync(PersonSearchDto searchDto, PagedRequestDto pageRequest)
    {
        try
        {
            var (items, totalCount) = await _personRepository.SearchAsync(
                searchDto.Name,
                searchDto.Email,
                searchDto.MinAge,
                searchDto.MaxAge,
                searchDto.IsActive,
                pageRequest.Page,
                pageRequest.PageSize,
                pageRequest.SortBy ?? "Id",
                pageRequest.SortDirection ?? "asc");

            var responseDtos = items.Select(MapToResponseDto);

            var pagedResult = new PagedResultDto<PersonResponseDto>
            {
                Data = responseDtos,
                TotalCount = totalCount,
                Page = pageRequest.Page,
                PageSize = pageRequest.PageSize
            };

            return ApiResponseDto<PagedResultDto<PersonResponseDto>>.SuccessResult(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching persons");
            return ApiResponseDto<PagedResultDto<PersonResponseDto>>.ErrorResult("An error occurred while searching persons");
        }
    }

    public async Task<ApiResponseDto<PersonResponseDto>> CreateAsync(PersonCreateDto createDto)
    {
        try
        {
            // Check if email already exists
            if (await _personRepository.EmailExistsAsync(createDto.Email))
            {
                return ApiResponseDto<PersonResponseDto>.ErrorResult("Email already exists");
            }

            var person = new Person
            {
                Name = createDto.Name.Trim(),
                Email = createDto.Email.Trim().ToLowerInvariant(),
                Age = createDto.Age,
                Phone = createDto.Phone?.Trim() ?? string.Empty,
                Address = createDto.Address?.Trim() ?? string.Empty,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            await _personRepository.AddAsync(person);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Person created successfully with ID {PersonId}", person.Id);

            var responseDto = MapToResponseDto(person);
            return ApiResponseDto<PersonResponseDto>.SuccessResult(responseDto, "Person created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating person");
            return ApiResponseDto<PersonResponseDto>.ErrorResult("An error occurred while creating the person");
        }
    }

    public async Task<ApiResponseDto<PersonResponseDto>> UpdateAsync(int id, PersonUpdateDto updateDto)
    {
        try
        {
            if (id <= 0)
            {
                return ApiResponseDto<PersonResponseDto>.ErrorResult("Invalid ID provided");
            }

            var person = await _personRepository.GetByIdAsync(id);
            if (person == null)
            {
                return ApiResponseDto<PersonResponseDto>.ErrorResult("Person not found");
            }

            // Check if email already exists (excluding current person)
            if (await _personRepository.EmailExistsAsync(updateDto.Email, id))
            {
                return ApiResponseDto<PersonResponseDto>.ErrorResult("Email already exists");
            }

            // Update properties
            person.Name = updateDto.Name.Trim();
            person.Email = updateDto.Email.Trim().ToLowerInvariant();
            person.Age = updateDto.Age;
            person.Phone = updateDto.Phone?.Trim() ?? string.Empty;
            person.Address = updateDto.Address?.Trim() ?? string.Empty;
            person.UpdatedDate = DateTime.UtcNow;

            _personRepository.Update(person);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Person updated successfully with ID {PersonId}", person.Id);

            var responseDto = MapToResponseDto(person);
            return ApiResponseDto<PersonResponseDto>.SuccessResult(responseDto, "Person updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating person with ID {PersonId}", id);
            return ApiResponseDto<PersonResponseDto>.ErrorResult("An error occurred while updating the person");
        }
    }

    public async Task<ApiResponseDto<bool>> DeleteAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                return ApiResponseDto<bool>.ErrorResult("Invalid ID provided");
            }

            var person = await _personRepository.GetByIdAsync(id);
            if (person == null)
            {
                return ApiResponseDto<bool>.ErrorResult("Person not found");
            }

            _personRepository.Remove(person); // This will trigger soft delete in DbContext
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Person deleted successfully with ID {PersonId}", id);

            return ApiResponseDto<bool>.SuccessResult(true, "Person deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting person with ID {PersonId}", id);
            return ApiResponseDto<bool>.ErrorResult("An error occurred while deleting the person");
        }
    }

    public async Task<ApiResponseDto<bool>> ExistsAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                return ApiResponseDto<bool>.SuccessResult(false);
            }

            var exists = await _personRepository.ExistsAsync(p => p.Id == id);
            return ApiResponseDto<bool>.SuccessResult(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if person exists with ID {PersonId}", id);
            return ApiResponseDto<bool>.ErrorResult("An error occurred while checking person existence");
        }
    }

    public async Task<ApiResponseDto<bool>> EmailExistsAsync(string email, int? excludeId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return ApiResponseDto<bool>.SuccessResult(false);
            }

            var exists = await _personRepository.EmailExistsAsync(email.Trim().ToLowerInvariant(), excludeId);
            return ApiResponseDto<bool>.SuccessResult(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if email exists: {Email}", email);
            return ApiResponseDto<bool>.ErrorResult("An error occurred while checking email existence");
        }
    }

    private static PersonResponseDto MapToResponseDto(Person person)
    {
        return new PersonResponseDto
        {
            Id = person.Id,
            Name = person.Name,
            Email = person.Email,
            Age = person.Age,
            Phone = person.Phone,
            Address = person.Address,
            CreatedDate = person.CreatedDate,
            IsActive = person.IsActive
        };
    }
}