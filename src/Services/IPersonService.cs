using BadApiExample.DTOs;

namespace BadApiExample.Services;

public interface IPersonService
{
    Task<ApiResponseDto<PersonResponseDto>> GetByIdAsync(int id);
    Task<ApiResponseDto<PagedResultDto<PersonResponseDto>>> GetAllAsync(PagedRequestDto request);
    Task<ApiResponseDto<PagedResultDto<PersonResponseDto>>> SearchAsync(PersonSearchDto searchDto, PagedRequestDto pageRequest);
    Task<ApiResponseDto<PersonResponseDto>> CreateAsync(PersonCreateDto createDto);
    Task<ApiResponseDto<PersonResponseDto>> UpdateAsync(int id, PersonUpdateDto updateDto);
    Task<ApiResponseDto<bool>> DeleteAsync(int id);
    Task<ApiResponseDto<bool>> ExistsAsync(int id);
    Task<ApiResponseDto<bool>> EmailExistsAsync(string email, int? excludeId = null);
}