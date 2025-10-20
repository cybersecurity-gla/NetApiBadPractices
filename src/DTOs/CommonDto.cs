using System.ComponentModel.DataAnnotations;

namespace BadApiExample.DTOs;

public class PagedResultDto<T>
{
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

public class PagedRequestDto
{
    private int _page = 1;
    private int _pageSize = 10;

    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page 
    { 
        get => _page; 
        set => _page = value < 1 ? 1 : value; 
    }

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize 
    { 
        get => _pageSize; 
        set => _pageSize = value < 1 ? 10 : (value > 100 ? 100 : value); 
    }

    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "asc";
}

public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IEnumerable<string>? Errors { get; set; }

    public static ApiResponseDto<T> SuccessResult(T data, string message = "Success")
    {
        return new ApiResponseDto<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponseDto<T> ErrorResult(string message, IEnumerable<string>? errors = null)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}